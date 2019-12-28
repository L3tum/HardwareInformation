#region using

using Reinforced.Typings.Attributes;

#endregion

namespace HardwareInformation.Information
{
	[TsClass]
	public class Display
	{
		/// <summary>
		///     Manufacturer of the monitor
		/// </summary>
		public string Manufacturer { get; set; }

		/// <summary>
		///     Name of the monitor
		/// </summary>
		public string Name { get; set; }
	}
}