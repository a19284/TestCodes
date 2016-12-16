namespace IBM.WMQ.Nmqi
{
    using IBM.WMQ;
    using System;
    using System.Net.Sockets;

    internal class MQPlainSocket : NmqiObject, IMQNetworkModule, IDisposable
    {
        private NmqiEnvironment env;
        private MQTCPConnection owningConnection;
        private const string sccsid = "%Z% %W% %I% %E% %U%";
        private Socket stream;

        public MQPlainSocket(NmqiEnvironment env, MQTCPConnection conn, Socket socket) : base(env)
        {
            base.TrConstructor("%Z% %W% %I% %E% %U%");
            this.stream = socket;
            this.env = env;
            this.owningConnection = conn;
        }

        public void Close()
        {
            uint method = 0x633;
            this.TrEntry(method);
            try
            {
                this.Dispose(true);
                base.TrText(method, "MQPlainSocket.Close completed");
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose()
        {
            uint method = 0x634;
            this.TrEntry(method);
            try
            {
                this.Dispose(false);
                base.TrText(method, "MQPlainSocket.Dispose completed");
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
        }

        public void Dispose(bool supressFinalize)
        {
            uint method = 0x634;
            this.TrEntry(method);
            if ((this.stream != null) && this.stream.Connected)
            {
                try
                {
                    this.stream.Shutdown(SocketShutdown.Both);
                    base.TrText(method, "MQPlain.SocketShutdown completed");
                }
                catch (SocketException exception)
                {
                    base.TrException(method, exception);
                }
                catch (ObjectDisposedException exception2)
                {
                    base.TrException(method, exception2);
                }
                try
                {
                    this.stream.Close();
                    base.TrText(method, "MQPlainSocket.Stream closed");
                }
                catch (ObjectDisposedException exception3)
                {
                    base.TrException(method, exception3);
                }
            }
            base.TrExit(method);
            if (supressFinalize)
            {
                GC.SuppressFinalize(this);
            }
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            uint method = 0x631;
            this.TrEntry(method);
            int num2 = -1;
            try
            {
            Label_000F:
                base.TrText(method, "Reading data from Socket");
                if (this.stream.Poll(this.owningConnection.timeout, SelectMode.SelectRead))
                {
                    num2 = this.stream.Receive(buffer, offset, length, SocketFlags.None);
                    base.TrText(method, "MQPlainSocket.Read completed with " + num2 + "bytes");
                    return num2;
                }
                if (this.stream.Connected)
                {
                    base.TrText(method, "Socket polling returned false,Socket is connected but Data available to read:" + this.stream.Available);
                    goto Label_000F;
                }
                num2 = -1;
                base.TrText(method, "Socket status: NOT CONNECTED, returning no data can be read");
                return num2;
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }

        public int Write(byte[] buffer, int offset, int length)
        {
            uint method = 0x632;
            int num2 = -1;
            this.TrEntry(method);
            try
            {
                num2 = this.stream.Send(buffer, offset, length, SocketFlags.None);
                base.TrText(method, "MQPlainSocket.Write completed with " + num2 + "bytes");
            }
            catch (Exception exception)
            {
                base.TrException(method, exception);
                throw exception;
            }
            finally
            {
                base.TrExit(method);
            }
            return num2;
        }
    }
}

