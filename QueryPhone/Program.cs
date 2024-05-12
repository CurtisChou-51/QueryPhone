using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
			services.AddSingleton<Form1>();
			services.AddSingleton<IQueryPhoneClient, PhoneBookClient>();
			services.AddSingleton<IQueryPhoneClient, TellowsClient>();
			services.AddSingleton<IQueryPhoneClient, WhocallClient>();
			services.AddSingleton<IQueryPhoneClient, WhosNumberClient>();

			ApplicationConfiguration.Initialize();
			Application.Run(services.BuildServiceProvider().GetRequiredService<Form1>());
		}
	}
}