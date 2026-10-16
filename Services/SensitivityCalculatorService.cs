using System;

namespace R6Tools.Services
{
	public class SensitivityConversionResult
	{
		public int NewSensitivity { get; set; }
		public double ExactSensitivity { get; set; }
		public double ErrorPercentage { get; set; }
		public bool IsOutOfRange { get; set; }
		public string WarningMessage { get; set; } = string.Empty;
	}

	public class SensitivityCalculatorService
	{
		// 시즈 엔진의 기본 yaw 스케일 계수 (180 / PI * 0.005)
		private const double EngineYawConstant = 0.2864788975654116;

		public SensitivityConversionResult ConvertSensitivity(int oldSens, double oldMultiplier, double newMultiplier)
		{
			var result = new SensitivityConversionResult();

			if (oldSens <= 0 || oldMultiplier <= 0 || newMultiplier <= 0)
			{
				result.NewSensitivity = Math.Max(1, oldSens);
				result.ExactSensitivity = result.NewSensitivity;
				return result;
			}

			double exact = oldSens * (oldMultiplier / newMultiplier);
			int rounded = (int)Math.Round(exact, MidpointRounding.AwayFromZero);

			result.ExactSensitivity = exact;

			if (rounded < 1)
			{
				result.NewSensitivity = 1;
				result.IsOutOfRange = true;
				result.WarningMessage = "인게임 최소 감도(1)를 밑돕니다. 1로 보정됩니다.";
			}
			else if (rounded > 100)
			{
				result.NewSensitivity = 100;
				result.IsOutOfRange = true;
				result.WarningMessage = "인게임 최대 감도(100)를 초과합니다. 100으로 제한됩니다.";
			}
			else
			{
				result.NewSensitivity = rounded;
				result.IsOutOfRange = false;
				result.WarningMessage = string.Empty;
			}

			if (exact > 0)
			{
				result.ErrorPercentage = Math.Abs((result.NewSensitivity - exact) / exact) * 100.0;
			}

			return result;
		}

		public double CalculateCm360(int yawSensitivity, double multiplier, int dpi)
		{
			if (yawSensitivity <= 0 || multiplier <= 0 || dpi <= 0)
			{
				return 0.0;
			}

			double degreesPerCount = yawSensitivity * multiplier * EngineYawConstant;
			double denominator = dpi * degreesPerCount;

			if (denominator <= 0)
			{
				return 0.0;
			}

			return 914.4 / denominator;
		}

		public double CalculateInches360(double cm360)
		{
			return cm360 <= 0 ? 0.0 : cm360 / 2.54;
		}

		public int CalculateEdpi(int dpi, int yawSensitivity)
		{
			return dpi * yawSensitivity;
		}
	}
}
