using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscRipper
{
    namespace TheDiscDb2
    {
        namespace GraphQL
        {
            public class Root
            {
                public Data Data { get; set; }
            }

            /// <summary>
            /// Top level of the GraphQL response
            /// </summary>
            public class Data
            {
                public MediaItems MediaItems { get; set; }
            }

            public class MediaItems
            {
                public List<Node> Nodes { get; set; }
            }

            /// <summary>
            /// Represents a particular movie or tv show
            /// </summary>
            public class Node
            {
                public string Title { get; set; }
                public int Year { get; set; }
                public List<Release> Releases { get; set; }

                [JsonIgnore]
                public string DirectoryName => $"{new string([.. Title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)])} ({Year})";
            }

            /// <summary>
            /// Each film or tv show may have multiple releases (dvd/bluray theatrical/collectors/extended
            /// </summary>
            public class Release
            {
                public string Title { get; set; }
                public int Year { get; set; }
                public string ImageUrl { get; set; }
                public List<Disc> Discs { get; set; }
            }

            /// <summary>
            /// A disc from a particular release. Each release may have many discs (4k/1080p/bonus content)
            /// </summary>
            public class Disc
            {
                public int Id { get; set; }
                public int Index { get; set; }
                public string Slug { get; set; }
                public string Name { get; set; }
                public string Format { get; set; }
                public List<Title> Titles { get; set; }
            }

            /// <summary>
            /// "Titles" on a disc, each bluray or dvd is made up of multiple titles.
            /// </summary>
            [DebuggerDisplay("{Item != null ? Item.Title : \"Null\"} ({Duration})")]
            public class Title
            {
                public string Duration { get; set; }
                public string ItemType { get; set; }
                public Item Item { get; set; }

                // Not serialized by json, but calculated later
                [JsonIgnore]
                public int DurationInSeconds { get; set; }
            }

            /// <summary>
            /// Some title metadata
            /// </summary>
            public class Item
            {
                public string Title { get; set; }
                public string Type { get; set; }

                [JsonIgnore]
                public string Filename => new string([.. Title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)]) + ".mkv";
            }
        }

        internal class Querier
        {
            #region Public properties

            public List<GraphQL.Node> Nodes { get; set; } = [];

            #endregion Public properties

            #region Public methods

            public async Task Query(string? duration)
            {
                if (duration == null)
                    return;

                var gqlQuery = new
                {
                    query = """
                    query {
                      mediaItems(
                        where: {
                          releases: {
                            some: {
                              discs: {
                                some: {
                                  titles: {
                                    some: { duration: { eq: "XXX" } }
                                  }
                                }
                              }
                            }
                          }
                        }
                      ) {
                        nodes {
                          title
                          year
                          releases {
                            title
                            year
                            imageUrl
                            discs {
                              id
                              index
                              slug
                              name
                              format
                              titles {
                                duration
                                itemType
                                item {
                                  title
                                  type
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                    """.Replace("XXX", duration),
                    variables = new { }
                };

                using HttpClient client = new()
                {
                    BaseAddress = new Uri("https://thediscdb.com/graphql")
                };

                string json = JsonSerializer.Serialize(gqlQuery);
                HttpContent c = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(string.Empty, c);
                var responseJson = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    using (var reader = new StreamReader(responseJson, Encoding.UTF8))
                    {
                        string value = reader.ReadToEnd();
                        string sentContent = await response.RequestMessage.Content.ReadAsStringAsync();

                        Debug.WriteLine($"Value: {value}");
                        Debug.WriteLine($"Request: {sentContent}");
                    }

                    return;
                }

                JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
                options.PropertyNameCaseInsensitive = true;

                GraphQL.Root deserializedObject = JsonSerializer.Deserialize<GraphQL.Root>(responseJson, options);

                if(deserializedObject.Data == null)
                {
                    responseJson.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(responseJson, Encoding.UTF8))
                    {
                        string value = reader.ReadToEnd();
                        Debug.WriteLine($"Value: {value}");
                    }
                    return;
                }

                Nodes = deserializedObject?.Data.MediaItems.Nodes;

                Extrapolate();
            }

            #endregion Public methods

            #region Private methods

            private void Extrapolate()
            {
                if (Nodes == null)
                    return;

                foreach(var node in Nodes)
                {
                    foreach( var release in node.Releases)
                    {
                        foreach(var disc in release.Discs)
                        {
                            foreach(var title in disc.Titles)
                            {
                                title.DurationInSeconds = (int)TimeSpan.Parse(title.Duration).TotalSeconds;
                            }
                        }
                    }
                }
            }

            #endregion Private methods
        }
    }
}
