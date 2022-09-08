using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiriusXMPlayer
{
	public class MainWindowViewModel : NinjaMvvm.NotificationBase
	{
		private readonly ILogger<MainWindowViewModel> _logger;

		public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
			IOptions<PlayerSettings> playerSettings)
		{
			_logger = logger;

			this.SiteUrl = playerSettings.Value.SiriusXMStartupUrl;
			_logger.LogInformation("ViewModel Ready, site is {siteurl}", this.SiteUrl);
		}


		public string SiteUrl
		{
			get { return GetField<string>(); }
			set { SetField(value); }
		}

	}
}
