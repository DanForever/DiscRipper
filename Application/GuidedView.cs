using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.Sessions;
using DiscRipper.ViewModel;

namespace DiscRipper.Guided
{
	internal interface StepControlFactory
	{
		public Control Create();
	}

	internal sealed class AsinControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.Asin();
		}
	}

	internal sealed class MediaTypeControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.MediaType();
		}
	}

	internal sealed class UpcControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.Upc();
		}
	}

	internal sealed class TmdbControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.Tmdb();
		}
	}

	internal sealed class PublicationDateControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.PublicationDate();
		}
	}

	internal sealed class CoverArtControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.CoverArt();
		}
	}

	internal sealed class RegionCodeControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.RegionCode();
		}
	}

	internal sealed class LocalesControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.Locale();
		}
	}

	internal sealed class DiscFormatControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.DiscFormat();
		}
	}

	internal sealed class DiscNameControlFactory : StepControlFactory
	{
		public Control Create()
		{
			return new Controls.Guided.DiscName();
		}
	}
	

	internal record Step
	{
		public string Title { get; init; }
		public string Description { get; init; }
		public StepControlFactory InnerContentControlFactory { get; init; }
	}

	internal class View
	{
		static readonly private string[] stepIconNames_ = ["OneIconTemplate", "TwoIconTemplate", "ThreeIconTemplate", "FourIconTemplate", "FiveIconTemplate", "SixIconTemplate", "SevenIconTemplate", "EightIconTemplate", "NineIconTemplate"];
		static readonly private string[] stepTitleStrings_ = ["Step One", "Step Two", "Step Three", "Step Four", "Step Five", "Step Six", "Step Seven", "Step Eight", "Step Nine", "Step Ten"];

		static readonly private Step[] _steps = new[]
		{
			new Step
			{
				Title = "TMDb ID",
				Description = "The first bit of information we require is the ID for the show or movie you want to submit to TheDiscDb. This simply involves going to the website (which you can do by clicking the button below), and then navigating to the page for the movie or show in question. After that, you can copy and paste the URL into the text field below.",
				InnerContentControlFactory = new TmdbControlFactory(),
			},
			new Step
			{
				Title = "Media Type",
				Description = """
					Does this disc contain a movie (or movie extras), or is it part of a TV series/show?
					If, in the previous step, you copied and pasted the URL of the TMDB page, then we've probably automatically filled this in already, in that case, just verify that we got it right and move on to the next step.
					""",
				InnerContentControlFactory = new MediaTypeControlFactory(),
			},
			new Step
			{
				Title = "UPC",
				Description = """
					Here we want the Universal product code, or "UPC". This should be the number underneath the barcode on the back of the box. It should also be possible to find this on the amazon product page under "Manufacturer reference" in the "Product details" section.
					""",
				InnerContentControlFactory = new UpcControlFactory(),
			},
			new Step
			{
				Title = "ASIN",
				Description = """
					Looking for another form of ID, but this time it's unique to amazon - "Amazon Standard Identification Number".
					Like the UPC, this one can be also found on the amazon product page in the "Product details" section, simply labelled "ASIN".
					However, again you can also simply copy and paste the url to the amazon product page directly into the textbox below and we'll extract it from that automatically.
					""",
				InnerContentControlFactory = new AsinControlFactory(),
			},
			new Step
			{
				Title = "Publication Date",
				Description = """
					The date that this particular disc edition was released (so not the date for when the movie first entered cinemas).
					Just like previously, it should be possible to find this information on the amazon product page under "Release date" in the "Product details" section.
					""",
				InnerContentControlFactory = new PublicationDateControlFactory(),
			},
			new Step
			{
				Title = "Cover Art",
				Description = """
					Here we would like you to specify urls to where we can download images for the front and back cover of the box for this release.
					Search for the cover art on the internet, and when you find it, right click -> "copy image link", and then paste that in the appropriate field below.
					Usually, the artwork should be available on the amazon product site, but if it's not there, you may also have luck at blu-ray.com.
					""",
				InnerContentControlFactory = new CoverArtControlFactory(),
			},
			new Step
			{
				Title = "Region Code",
				Description = """
					Is this disc locked to a specific region? Usually you can find the region code on the back of the box, it will be a letter (A, B or C) inside a globe icon. If there is no such icon, then the disc is most likely region free.
					Sites like blu-ray.com usually also have the region code listed on the product page, so if you have trouble finding it on the box, you can also try looking there.

					You can find out more about the different region codes and what they mean at Wikipedia.
					""",
				InnerContentControlFactory = new RegionCodeControlFactory(),
			},
			new Step
			{
				Title = "Locale",
				Description = """
					Here we want the country and (primary) language for this release. Not the country of origin for the film, but this particular disc release.
					
					If the blurb on the box is in German, and the age rating is "FSK" then you probably want to put "German (Germany)". If it's in English and you have BBFC age ratings, then you more than likely want "English (United Kingdom)".
					""",
				InnerContentControlFactory = new LocalesControlFactory(),
			},
			new Step
			{
				Title = "Disc format",
				Description = """
					What kind of disc is this?

					If it's a 4k Blu-ray, then you should select "UHD".
					""",
				InnerContentControlFactory = new DiscFormatControlFactory(),
			},
			new Step
			{
				Title = "Disc name",
				Description = """
				A unique name for this disc (of possibly many discs from this release).

				This is up to you, you could name them simply "disc1, disc2, disc3, etc", or if it's a 4k release (which also has the 1080p on another disc), you could name them after the formats: "uhd", "blu-ray", "bonus-content".
				The name needs to uniquely identify this disc from all the other discs in this release, you don't need to care about other discs from other releases having the same name.
				""",
				InnerContentControlFactory = new DiscNameControlFactory(),
			},
		};

		public string[] StepTitleStrings => stepTitleStrings_;
		public static Step[] Steps => _steps;
		public int CurrentStepIndex { get; private set; } = 0;

		public required Window Owner { get; init; }
		public required Submission Submission { get; init; }
		public required Session Session { get; init; }
		public required MakeMkv.Log Log { get; init; }

		public void ShowFirst()
		{
			CurrentStepIndex = 0;

			ShowCurrentStep(Owner, null, null);
		}

		public bool ShowNext(double? left, double? top)
		{
			++CurrentStepIndex;

			return ShowCurrentStep(Owner, left, top);
		}

		public bool ShowPrevious(double? left, double? top)
		{
			--CurrentStepIndex;

			return ShowCurrentStep(Owner, left, top);
		}

		private bool ShowCurrentStep(Window owner, double? left, double? top)
		{
			if (CurrentStepIndex < 0)
				return false;

			if (CurrentStepIndex >= Steps.Length)
				return false;

			ShowStep(Steps[CurrentStepIndex], owner, left, top);

			return true;
		}

		private void ShowStep(Step step, Window owner, double? left, double? top)
		{
			Windows.Base.GuidedStep stepWindow = new Windows.Base.GuidedStep
			{
				StepNumberIcon = (CurrentStepIndex < stepIconNames_.Length) ? (ControlTemplate)Application.Current.FindResource(stepIconNames_[CurrentStepIndex]) : null,
				StepNumber = StepTitleStrings[CurrentStepIndex],
				Title = step.Title,
				Description = step.Description,
				InnerContent = step.InnerContentControlFactory.Create(),

				Submission = Submission,
				Session = Session,
				Log = Log,

				Owner = owner,
				DataContext = Submission,

				HasPrevious = CurrentStepIndex > 0,
				Left = left ?? 0,
				Top = top ?? 0,
				WindowStartupLocation = left.HasValue && top.HasValue ? WindowStartupLocation.Manual : WindowStartupLocation.CenterOwner
			};

			stepWindow.NextClicked += (sender, args) =>
			{
				var window = Window.GetWindow((DependencyObject)sender);
				window.Close();

				ShowNext(window.Left, window.Top);
			};

			stepWindow.PreviousClicked += (sender, args) =>
			{
				var window = Window.GetWindow((DependencyObject)sender);
				window.Close();

				ShowPrevious(window.Left, window.Top);
			};

			stepWindow.ShowDialog();
		}
	}
}
