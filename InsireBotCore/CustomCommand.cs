﻿using GalaSoft.MvvmLight;
using System;

namespace InsireBotCore
{
	/// <summary>
	/// storage element für a custom command 
	/// </summary>
	public class CustomCommand : ObservableObject
	{
		public String Response { get; set; }
		public String Command { get; set; }

		public object Value { get; set; }
	}
}