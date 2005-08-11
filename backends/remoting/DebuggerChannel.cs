using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Debugger.Remoting
{
	public class DebuggerChannel : IChannelReceiver, IChannelSender, IChannel
	{
		DebuggerConnection connection;
		DebuggerServerChannel server_channel;
		DebuggerClientChannel client_channel;

		public DebuggerChannel (string url)
		{
			server_channel = new DebuggerServerChannel (url);
			connection = new DebuggerConnection (server_channel, 3);
			client_channel = new DebuggerClientChannel (server_channel, connection);
		}

		public DebuggerChannel ()
		{
			server_channel = new DebuggerServerChannel (null);
			client_channel = new DebuggerClientChannel (server_channel);
		}

		public string ChannelName {
			get { return "mdb"; }
		}

		public int ChannelPriority {
			get { return 1; }
		}

		public DebuggerConnection Connection {
			get { return connection; }
		}

		public IMessageSink CreateMessageSink (string url, object remoteChannelData, out string objectURI)
		{
			return client_channel.CreateMessageSink (url, remoteChannelData, out objectURI);
		}

		public void StartListening (object data)
		{
			server_channel.StartListening (data);
		}

		public void StopListening (object data)
		{
			server_channel.StopListening (data);
		}

		public string[] GetUrlsForUri (string uri)
		{
			return server_channel.GetUrlsForUri (uri);
		}

		public object ChannelData {
			get {
				return server_channel.ChannelData;
			}
		}

		public string Parse (string url, out string objectURI)
		{
			string host, path;
			return DebuggerChannel.ParseDebuggerURL (url, out host, out path, out objectURI);
		}

		internal static string ParseDebuggerURL (string url, out string host, out string path,
							 out string objectURI)
		{
			objectURI = null;
			host = null;
			path = null;

			if (!url.StartsWith ("mdb://"))
				return null;

			int pos = url.IndexOf ('!', 6);
			if (pos == -1) return null;
			path = url.Substring (6, pos - 6);

			objectURI = url.Substring (pos + 1);

			int colon = path.IndexOf (':');
			if (colon > 0) {
				host = path.Substring (0, colon);
				path = path.Substring (colon + 1);
			}

			if (host != null)
				return "mdb://" + host + ":" + path;
			else
				return "mdb://" + path;
		}
	}
}
