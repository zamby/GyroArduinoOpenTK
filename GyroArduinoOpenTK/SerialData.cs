using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GyroArduinoOpenTK
{
    public interface ISerialData
    {
        bool work { get; set; }
        string sendString { get; set; }
        SerialPort serialPort { get; set; }
        event SerialData.DataHandler EventDataReceive;
        void start();
        void stop();
    }

    public class DataArray
    {
        public float angleX { get; set; }
        public float angleY { get; set; }
        public float angleZ { get; set; }
        public float gyroX { get; set; }
        public float gyroY { get; set; }
        public float gyroZ { get; set; }
        public float gravityX { get; set; }
        public float gravityY { get; set; }
        public float gravityZ { get; set; }

        public override string ToString()
        {
            return string.Format(@"[SerialData: angleX={0}, angleY={1}, angleZ={2}, gyroX={0}, gyroY={1}, gyroZ={2}, gravityX={0}, gravityY={1}, gravityZ={2} ]", this.angleX, this.angleY, this.angleZ, this.gyroX, this.gyroY, this.gyroZ, this.gravityX, this.gravityY, this.gravityZ);
        }
    }

    public class EmuSerialData : ISerialData
    {
        public bool work { get; set; } = true;
        public string sendString { get; set; }
        public SerialPort serialPort { get; set; }

        public event SerialData.DataHandler EventDataReceive;

        public void start()
        {
            Task.Factory.StartNew(() =>
            {
                double i = 0;
                while (work)
                {
                    i = i + 0.00001;
                    Task.Delay(80);
                    DataArray da = new DataArray();
                    da.angleX = (float)Math.Cos(i);
                    da.angleY = (float)Math.Sin(i);
                    da.angleZ = da.angleX * (float)Math.Sin(i);
                    EventDataReceive(da);

                }
            });
        }

        public void stop()
        {
            work = false;

        }
    }

    public class SerialData : ISerialData
    {
        public bool work { get; set; } = true;
        public SerialPort serialPort { get; set; }
        public string sendString { get; set; }

        public delegate void DataHandler(DataArray data);
        public event DataHandler EventDataReceive;

        public SerialData(string port, int baud)
        {

            try
            {

                serialPort = new SerialPort(port, baud);

                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }

                serialPort.Open();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void start()
        {

            if (serialPort == null || !serialPort.IsOpen)
                return;


            Task.Factory.StartNew(() =>
            {
                while (work)
                {

                    if (!String.IsNullOrEmpty(sendString))
                    {
                        serialPort.WriteLine(sendString);
                        sendString = "";
                    }
                    else
                    {
                        string d = serialPort.ReadLine();
                        if (EventDataReceive != null)
                        {
                            if (d.Length > 5 && d[0].Equals('@') && d[d.Length - 2].Equals('@'))
                            {
                                string[] das = d.Substring(1, d.Length - 3).Split('|');
                                if (das.Length == 6)
                                {
                                    DataArray da = new DataArray();
                                    da.angleX = float.Parse(das[0], CultureInfo.InvariantCulture);
                                    da.angleY = float.Parse(das[1], CultureInfo.InvariantCulture);
                                    da.angleZ = float.Parse(das[2], CultureInfo.InvariantCulture);
                                    //da.gyroX = float.Parse (das [3],CultureInfo.InvariantCulture);
                                    //da.gyroY = float.Parse (das [4],CultureInfo.InvariantCulture);
                                    //da.gyroZ = float.Parse (das [5],CultureInfo.InvariantCulture);
                                    da.gravityX = float.Parse(das[3], CultureInfo.InvariantCulture);
                                    da.gravityY = float.Parse(das[4], CultureInfo.InvariantCulture);
                                    da.gravityZ = float.Parse(das[5], CultureInfo.InvariantCulture);
                                    EventDataReceive(da);
                                }
                            }
                        }
                    }
                }
            });

        }

        public void stop()
        {
            work = false;

        }

    }

}