﻿/*
 *  Copyright © 2013-2015, Andrew Nagle.
 *  All rights reserved.
 *
 *  This file is part of OracleHack.
 *
 *  OracleHack is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  OracleHack is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with OracleHack. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zyrenth.OracleHack
{
	/// <summary>
	/// Represents a secret used in the Zelda Oracle series games.
	/// </summary>
	public abstract class Secret : INotifyPropertyChanged
	{
		private static readonly byte[] Cipher =
		{ 
			21, 35, 46,  4, 13, 63, 26, 16,
			58, 47, 30, 32, 15, 62, 54, 55,
			 9, 41, 59, 49,  2, 22, 61, 56, 
			40, 19, 52, 50,  1, 11, 10, 53,
			14, 27, 18, 44, 33, 45, 37, 48,
			25, 42,  6, 57, 60, 23, 51, 24
		};

		short _gameId = 0;

		/// <summary>
		/// Occurs when a property has changed
		/// </summary>
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Gets the required length of the secret
		/// </summary>
		public abstract int Length { get; }

		/// <summary>
		/// Gets or sets the unique game ID 
		/// </summary>
		public short GameID
		{
			get { return _gameId; }
			set
			{
				_gameId = value;
				OnPropertyChanged("GameID");
			}
		}

		/// <summary>
		/// Loads in data from the specified game info
		/// </summary>
		/// <param name="info">The game info</param>
		public abstract void Load(GameInfo info);

		/// <summary>
		/// Loads in data from the raw secret data provided
		/// </summary>
		/// <param name="secret">The raw secret data</param>
		public abstract void Load(byte[] secret);

		/// <summary>
		/// Loads in data from the secret string provided
		/// </summary>
		/// <param name="secret">The secret</param>
		public virtual void Load(string secret)
		{
			Load(SecretParser.ParseSecret(secret));
		}

		/// <summary>
		/// Gets the raw secret data as a byte array
		/// </summary>
		/// <returns>A byte array containing the secret</returns>
		public abstract byte[] GetSecretBytes();

		/// <summary>
		/// Gets a string representation of the secret that's usable in the games
		/// </summary>
		/// <returns>A string representation of the string</returns>
		public virtual string GetSecretString()
		{
			return SecretParser.CreateString(GetSecretBytes());
		}

		/// <summary>
		/// Calculates the checksum for the specified <paramref name="secret"/>
		/// </summary>
		/// <param name="secret">The secret</param>
		/// <returns>The calculated checksum</returns>
		protected byte CalculateChecksum(byte[] secret)
		{
			// LINQ doesn't support .Sum() for anything smaller than an int, so we
			// have to use the lambda overload instead. Bytes get auto-promoted to
			// ints, which explains why we have to cast the return back to a byte.
			// Because reasons.
			byte sum = (byte)secret.Sum(x => x);
			int checksum = sum & 0x0F;
			return (byte)checksum;
		}

		/// <summary>
		/// Takes a raw, unencoded secret and encodes it.
		/// </summary>
		/// <param name="data">The secret data.</param>
		/// <returns>An encoded secret.</returns>
		protected byte[] EncodeBytes(byte[] data)
		{
			int cipherKey = (data[0] >> 3);
			int cipherPosition = cipherKey * 4;

			byte[] secret = new byte[data.Length];
			for (int i = 0; i < data.Length; ++i)
			{
				secret[i] = (byte)(data[i] ^ Cipher[cipherPosition++]);
			}

			secret[0] = (byte)(secret[0] & 7 | (cipherKey << 3));
			return secret;
		}

		/// <summary>
		/// Takes an encoded secret, as used by the games, and decodes it.
		/// </summary>
		/// <param name="secret">The encoded secret.</param>
		/// <returns>A decoded secret.</returns>
		protected byte[] DecodeBytes(byte[] secret)
		{
			int cipherKey = (secret[0] >> 3);
			int cipherPosition = cipherKey * 4;

			byte[] decodedBytes = new byte[secret.Length];

			for (int i = 0; i < secret.Length; ++i)
			{
				decodedBytes[i] = (byte)(secret[i] ^ Cipher[cipherPosition++]);
			}

			decodedBytes[0] = (byte)(decodedBytes[0] & 7 | (cipherKey << 3));

			return decodedBytes;
		}

		/// <summary>
		/// Converts a byte array to a string representation of ones and zeros
		/// </summary>
		/// <param name="secret">The secret.</param>
		/// <returns>A string of ones and zeros</returns>
		protected string ByteArrayToBinaryString(byte[] secret)
		{
			string data = "";
			foreach (byte b in secret)
			{
				data += Convert.ToString(b, 2).PadLeft(6, '0');
			}
			return data;
		}

		/// <summary>
		/// Converts a binary string (i.e. a string of ones and zeros) to a byte array
		/// </summary>
		/// <param name="data">The binary string</param>
		/// <returns>A byte array that the string represents</returns>
		protected byte[] BinaryStringToByteArray(string data)
		{
			byte[] secret = new byte[data.Length / 6 + 1];
			for (int i = 0; i < secret.Length - 1; ++i)
			{
				secret[i] = (byte)(Convert.ToByte(data.Substring(i * 6, 6), 2));
			}
			return secret;
		}

		/// <summary>
		/// Called when a property has changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this,
					new PropertyChangedEventArgs(propertyName));
			}
		}

	}
}
