using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using DiscRipper.MakeMkv;
namespace DiscRipper.Sessions
{
	public class Session
	{
		private string? _log;

		public Guid Id { get; init; } = Guid.NewGuid();
		public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
		public required TheDiscDb.Submission.Disc Disc { get; set; }
		public TheDiscDb.Submission.Release? Release { get; set; }
		public Types.Hash.Disc? DiscHash { get; set; }

		[XmlIgnore]
		public bool LogDirty { get; set; } = false;

		[XmlIgnore]
		public string? Log
		{
			get => _log;
			set
			{
				_log = value;
				if (!string.IsNullOrEmpty(_log))
				{
					LogDirty = true;
				}
			}
		}
	}

	public class SessionList
	{
		public List<Session> Sessions { get; init; } = [];
	}

	internal class SessionManager
	{
		public static readonly Lazy<SessionManager> Instance = new(() => new SessionManager());

		public SessionList SessionList { get; private set; } = new();

		public static string SessionsDirectoryPath => Path.Join(DiscRipperSettingsProvider.SettingsDirectoryPath, "sessions");
		public static string LogsDirectoryPath => Path.Join(SessionsDirectoryPath, "logs");
		public static string SessionsFilePath => Path.Join(SessionsDirectoryPath, "sessions.xml");

		public Session CreateSession(List<Types.Title> titles)
		{
			Session session = new()
			{
				Disc = new() { Titles = titles },
				Release = new() {}
			};
			SessionList.Sessions.Add(session);

			return session;
		}

		public Session CreateDiscOnlySession(List<Types.Title> titles)
		{
			Session session = new()
			{
				Disc = new() { Titles = titles },
			};
			SessionList.Sessions.Add(session);

			return session;
		}

		public void Save()
		{
			Directory.CreateDirectory(SessionsDirectoryPath);

			{
				var serializer = new System.Xml.Serialization.XmlSerializer(typeof(SessionList));
				using var writer = new StreamWriter(SessionsFilePath, false, Encoding.UTF8);
				serializer.Serialize(writer, SessionList);
			}

			Directory.CreateDirectory(LogsDirectoryPath);
			foreach (var session in SessionList.Sessions)
			{
				if (!session.LogDirty)
					continue;

				string logPath = GetLogPath(session);
				File.WriteAllText(logPath, session.Log);
				session.LogDirty = false;
			}
		}

		public async Task SaveAsync()
		{
			await Task.Run(() => Save());
		}

		public void Load()
		{
			if (!File.Exists(SessionsFilePath))
				return;

			var serializer = new XmlSerializer(typeof(SessionList));
			using (var stream = File.OpenRead(SessionsFilePath))
			{
				try
				{
					SessionList = (SessionList)serializer.Deserialize(stream);
				}
				catch (Exception ex) { }
			}
		}

		public async Task LoadAsync()
		{
			await Task.Run(() => Load());
		}

		public async Task<Log?> LoadLog(Session session)
		{
			string logPath = GetLogPath(session);

			string logText;

			try
			{
				logText = await File.ReadAllTextAsync(logPath);
			}
			catch (FileNotFoundException ex)
			{
				return null;
			}

			MakeMkv.Runner runner = new()
			{
				MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
				SynchronizationContext = SynchronizationContext.Current!,
			};

			await runner.RunPreloaded(logText);

			return runner.Log;
		}

		private static string GetLogPath(Session session)
		{
			string logPath = Path.Join(LogsDirectoryPath, $"{session.Id}.log");

			return logPath;
		}
	}
}
