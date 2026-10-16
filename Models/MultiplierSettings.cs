namespace R6Tools.Models
{
	public class MultiplierSettings
	{
		public int MouseYawSensitivity { get; set; } = 50;
		public int MousePitchSensitivity { get; set; } = 50;
		public double MouseSensitivityMultiplierUnit { get; set; } = 0.02;
		public double XFactorAiming { get; set; } = 0.02;
		public bool RawInputMouseKeyboard { get; set; } = true;

		public static MultiplierSettings Default => new()
		{
			MouseYawSensitivity = 50,
			MousePitchSensitivity = 50,
			MouseSensitivityMultiplierUnit = 0.02,
			XFactorAiming = 0.02,
			RawInputMouseKeyboard = true
		};

		public static MultiplierSettings ProStandard => new()
		{
			MouseYawSensitivity = 50,
			MousePitchSensitivity = 50,
			MouseSensitivityMultiplierUnit = 0.00223,
			XFactorAiming = 0.02,
			RawInputMouseKeyboard = true
		};

		public static MultiplierSettings UltraPrecise => new()
		{
			MouseYawSensitivity = 50,
			MousePitchSensitivity = 50,
			MouseSensitivityMultiplierUnit = 0.001,
			XFactorAiming = 0.02,
			RawInputMouseKeyboard = true
		};

		public static MultiplierSettings OneToOneAds => new()
		{
			MouseYawSensitivity = 50,
			MousePitchSensitivity = 50,
			MouseSensitivityMultiplierUnit = 0.02,
			XFactorAiming = 1.0,
			RawInputMouseKeyboard = true
		};
	}
}
