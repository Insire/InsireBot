﻿using GalaSoft.MvvmLight;
using InsireBot.Interfaces;
using System;

namespace InsireBot.Objects
{
	/// <summary>
	/// storage element für a custom command 
	/// </summary>
	public class CustomCommand : ObservableObject
	{
		public String Response { get; set; }
		public String Command { get; set; }
	}
}