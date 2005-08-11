using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace Mono.Debugger.Remoting
{
	public class DebuggerClientTransportSinkProvider : IClientChannelSinkProvider
	{
		public IClientChannelSinkProvider Next {
			get {
				return null;
			}

			set {
				Console.Error.WriteLine ("NEXT SINK PROVIDER: {0}", value);
				// ignore, we are always the last in the chain 
			}
		}

		public IClientChannelSink CreateSink (IChannelSender channel, string url,
						      object remoteChannelData)
		{
			return new DebuggerClientTransportSink ((DebuggerClientChannel) channel, url);
		}
	}
}