﻿using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.User32_Gdi;

namespace Vanara.Windows.Shell
{
	/// <summary>Wraps a string resource reference used by some Shell classes.</summary>
	[TypeConverter(typeof(IndirectStringTypeConverter))]
	public class IndirectString : IndirectResource
	{
		/// <summary>Initializes a new instance of the <see cref="IndirectString"/> class.</summary>
		public IndirectString() { }

		/// <summary>Initializes a new instance of the <see cref="IndirectString"/> class.</summary>
		public IndirectString(string value) : base(value, 0) { }

		/// <summary>Initializes a new instance of the <see cref="IndirectString"/> class.</summary>
		/// <param name="module">The module file name.</param>
		/// <param name="resourceIdOrIndex">
		/// If this number is positive, this is the index of the resource in the module file. If negative, the absolute value of the number
		/// is the resource ID of the string in the module file.
		/// </param>
		public IndirectString(string module, int resourceIdOrIndex) : base(module, resourceIdOrIndex) { }

		/// <summary>Gets the raw value of the string.</summary>
		/// <value>Returns a <see cref="String"/> value.</value>
		[Browsable(false)]
		public string RawValue => IsValid ? $"@{ModuleFileName},{ResourceId}" : ModuleFileName;

		/// <summary>Gets the localized string referred to by this instance.</summary>
		/// <value>The referenced localized string.</value>
		[Browsable(false)]
		public string Value
		{
			get
			{
				if (ResourceId == 0) return ModuleFileName;
				using (var lib = LoadLibraryEx(ModuleFileName, Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE))
				{
					if (ResourceId >= 0) throw new NotSupportedException();
					const int sz = 2048;
					var sb = new System.Text.StringBuilder(sz, sz);
					LoadString(lib, -ResourceId, sb, sz);
					return sb.ToString();
				}
			}
		}

		/// <summary>Performs an implicit conversion from <see cref="IndirectString"/> to <see cref="System.String"/>.</summary>
		/// <param name="ind">The ind.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator string(IndirectString ind) => ind.RawValue;

		/// <summary>Performs an implicit conversion from <see cref="System.String"/> to <see cref="IndirectString"/>.</summary>
		/// <param name="s">The s.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator IndirectString(string s) => TryParse(s, out var loc) ? loc : null;

		/// <summary>Tries to parse the specified string to create a <see cref="IndirectString"/> instance.</summary>
		/// <param name="value">The string representation in the format of either "ModuleFileName,ResourceIndex" or "ModuleFileName,-ResourceID".</param>
		/// <param name="loc">The resulting <see cref="IndirectString"/> instance on success.</param>
		/// <returns><c>true</c> if successfully parsed.</returns>
		public static bool TryParse(string value, out IndirectString loc)
		{
			var parts = value?.Split(',');
			if (parts != null && parts.Length == 2 && int.TryParse(parts[1], out var i) && parts[0].StartsWith("@"))
			{
				loc = new IndirectString(parts[0].TrimStart('@'), i);
				return true;
			}
			loc = new IndirectString(value);
			return !string.IsNullOrEmpty(value);
		}

		/// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public override string ToString() => RawValue ?? "";
	}

	internal class IndirectStringTypeConverter : ExpandableObjectConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
		{
			if (destType == typeof(InstanceDescriptor) || destType == typeof(string))
				return true;
			return base.CanConvertTo(context, destType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string s)
				return IndirectString.TryParse(s, out var loc) ? loc : null;
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo info, object value, Type destType)
		{
			if (destType == typeof(string) && value is IndirectString s)
				return s.RawValue;
			if (destType == typeof(InstanceDescriptor))
				return new InstanceDescriptor(typeof(IndirectString).GetConstructor(new Type[0]), null, false);
			return base.ConvertTo(context, info, value, destType);
		}
	}
}