namespace TestApp
{
	using System;
	using System.IO;
	using System.Threading;

	using CellScanner.API;
	using CellScanner.Threading;

	internal class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Run();
			}
			catch (Exception ex)
			{
				LogError("ERROR: " + ex);
			}

			Console.WriteLine("DONE");
			Console.ReadKey();
		}

		private static void Run()
		{
			var thread = new CellScannerThread();

			thread.Invoke(() => CellScanner.DefineExternalLogMsg(Log));
			thread.Invoke(() => CellScanner.DefineExternalShowErrorMsg(LogError));
			thread.Invoke(() => CellScanner.DefineExternalGetMeasurement(GetMeasurement_CallBack));
			thread.Invoke(() => CellScanner.SetMeasurementInterval(5000));

			string ip = File.ReadAllText("IP Address.txt").Trim();
			Console.WriteLine($"IP address from file: {ip}");
			thread.Invoke(() => CellScanner.Set_IP_Addr(ip));

			thread.Invoke(() => CellScanner.Set_GPS(false));

			Log($"DLL version: {CellScanner.Get_DLL_Version()}");

			var freqs = BuildFrequenciesList();

			for (var i = 0; i < 1000; i++)
			{
				Console.WriteLine($"Iteration {i}");

				if (thread.Invoke(() => CellScanner.SetFrequencies(freqs)) != 0)
				{
					throw new Exception("Couldn't set the frequencies");
				}

				if (thread.Invoke(CellScanner.StartMeasurement) != 0)
				{
					throw new Exception("Couldn't start measurement");
				}

				// collect measurements for 10 seconds
				Thread.Sleep(10000);

				if (thread.Invoke(CellScanner.StopMeasurement) != 0)
				{
					throw new Exception("Couldn't stop measurement");
				}
			}
		}

		private static void GetMeasurement_CallBack(IntPtr meas_ptr)
		{
			var meas = TCellScannerMeasurement.FromPointer(meas_ptr);

			Log("Measurement (CB): " + meas.CommonData + " - " + meas.MeaInfo);
		}

		private static void Log(string log)
		{
			Console.WriteLine($"[{DateTime.Now}] {log}");
		}

		private static void LogError(string log)
		{
			Console.WriteLine($"[{DateTime.Now}] ERROR: {log}");
		}

		private static TScannerFreqList BuildFrequenciesList()
		{
			var freqs = new[]
			{
				new TScannerFrequency
				{
					Band = 1,
					ChannelNumber = 1,
					FreqMHz = 632.55,
					Tech = TCellScannerTechnology.CST_5GNR,
					LTE_DuplexMode = TDuplexingMode.DM_NotApplicable,
					NR5G_SCS = T_5GNR_SCS.NSCS_15kHz,
				},
				new TScannerFrequency
				{
					Band = 2,
					ChannelNumber = 2,
					FreqMHz = 751.00,
					Tech = TCellScannerTechnology.CST_LTE,
					LTE_DuplexMode = TDuplexingMode.DM_FDD,
					NR5G_SCS = T_5GNR_SCS.NSCS_15kHz,
				},
				new TScannerFrequency
				{
					Band = 3,
					ChannelNumber = 3,
					FreqMHz = 876.80,
					Tech = TCellScannerTechnology.CST_UMTS,
					LTE_DuplexMode = TDuplexingMode.DM_NotApplicable,
					NR5G_SCS = T_5GNR_SCS.NSCS_15kHz,
				},
				new TScannerFrequency
				{
					Band = 4,
					ChannelNumber = 4,
					FreqMHz = 1969.00,
					Tech = TCellScannerTechnology.CST_GSM,
					LTE_DuplexMode = TDuplexingMode.DM_NotApplicable,
					NR5G_SCS = T_5GNR_SCS.NSCS_15kHz,
				},
			};

			return new TScannerFreqList(freqs);
		}
	}
}
