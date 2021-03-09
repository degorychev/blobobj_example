using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace blobobj
{
    public partial class Form1 : Form
    {
        dbworker dbw = new dbworker("localhost", "root", "", "test");
        public Form1()
        {
            InitializeComponent();
            loadImages();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            Image img = Image.FromFile(ofd.FileName);
            dbw.insertImg(img, ofd.FileName);
        }

        private void loadImages()
        {
            var images = dbw.getimages();
            listBox1.DataSource = images;
            listBox1.ValueMember = "id";
            listBox1.DisplayMember = "name";
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = dbw.getImg(Convert.ToInt32(listBox1.SelectedValue));
            }
            catch(Exception ex)
            {
            }
        }
    }
    class dbworker
    {
        MySqlConnection Connection;
        MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
        public dbworker(string server, string user, string pass, string database)
        {
            mySqlConnectionStringBuilder.Server = server;
            mySqlConnectionStringBuilder.UserID = user;
            mySqlConnectionStringBuilder.Password = pass;
            mySqlConnectionStringBuilder.Port = 3306;
            mySqlConnectionStringBuilder.Database = database;
            mySqlConnectionStringBuilder.CharacterSet = "utf8";
            Connection = new MySqlConnection(mySqlConnectionStringBuilder.ConnectionString);
        }

        public void insertImg(Image img, string name)
        {
            var bytes = imageToByte(img);
            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO images(`name`, `img`, `ImageSize`) VALUES (@name, @image, @size);";

            var paramUserImage = new MySqlParameter("@image", MySqlDbType.Blob, bytes.Length);
            paramUserImage.Value = bytes;
            command.Parameters.Add(paramUserImage);
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
            command.Parameters.Add("@size", MySqlDbType.Int32).Value = bytes.Length;
            try
            {
                Connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                Connection.Close();
            }
        }
        public DataTable getimages()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT `id`, `name` FROM images";
            DataTable dt = new DataTable();
            try
            {
                Connection.Open();
                dt.Load(cmd.ExecuteReader());
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                Connection.Close();
            }
            return dt;
        }

        public Image getImg(int id)
        {
            byte[] rawData;
            UInt32 FileSize;
            Image outImage;


            var command = Connection.CreateCommand();
            command.CommandText = "SELECT `img`, `ImageSize` FROM images WHERE `id`=@id";
            command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
            try
            {
                Connection.Open();
                var myData = command.ExecuteReader();
                if (myData.Read())
                {
                    FileSize = myData.GetUInt32(myData.GetOrdinal("ImageSize"));
                    rawData = new byte[FileSize];

                    myData.GetBytes(myData.GetOrdinal("img"), 0, rawData, 0, (Int32)FileSize);
                    outImage = Image.FromStream(new MemoryStream(rawData));


                    myData.Close();
                    myData.Dispose();

                    command.Dispose();

                    return outImage;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Connection.Close();
            }
            return null;
        }

        private byte[] imageToByte(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private Image ImageFromByte(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
