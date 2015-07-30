﻿using System;

namespace VisualCrypt.Cryptography.Portable.APIV2.DataTypes
{
	public sealed class PasswordDerivedKey32 : SecureBytes
	{
		public PasswordDerivedKey32(byte[] dataBytes) : base(dataBytes)
		{
			// perform datatype-specific validation here
			if (dataBytes.Length != 32)
				throw new ArgumentOutOfRangeException("dataBytes", "The length must be 32 bytes.");
		}
	}
}