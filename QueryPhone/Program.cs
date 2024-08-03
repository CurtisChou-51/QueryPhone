using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QueryPhone.Clients;

namespace QueryPhone
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			
			var services = new ServiceCollection();
			services.AddLogging(builder =>
			{
				builder.AddDebug();
			});
			services.AddSingleton<Form1>();
			services.AddSingleton<IQueryPhoneClient, PhoneBookClient>();
			services.AddSingleton<IQueryPhoneClient, TellowsClient>();
			services.AddSingleton<IQueryPhoneClient, WhocallClient>();
			services.AddSingleton<IQueryPhoneClient, WhosNumberClient>();
			services.AddSingleton<IQueryPhoneClient, BaselyClient>();

			ApplicationConfiguration.Initialize();
			Application.Run(services.BuildServiceProvider().GetRequiredService<Form1>());
		}
	}
}