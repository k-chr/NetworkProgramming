﻿using System.IO;
using System.Net.Sockets;

namespace NetworkProgramming.Lab1
{
	public class ControlState
	{
		public Socket CurrentSocket = null;
		public long BufferSize { get; set; }
		public byte[] Buffer = null;
		public MemoryStream StreamBuffer = new MemoryStream();
	}
}