namespace TestApp
{
	using System;

	using CellScanner.API;
	using CellScanner.Threading;

	internal class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Run();;
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

			thread.Invoke(() => CellScanner.Set_IP_Addr("192.168.120.135"));
			thread.Invoke(() => CellScanner.Set_GPS(false));

			Log($"DLL version: {CellScanner.Get_DLL_Version()}");

			var freqs = BuildFrequenciesList();

			if (thread.Invoke(() => CellScanner.SetFrequencies(freqs)) != 0)
			{
				throw new Exception("Couldn't set the frequencies");
			}

			if (thread.Invoke(CellScanner.StartMeasurement) != 0)
			{
				throw new Exception("Couldn't start measurement");
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
