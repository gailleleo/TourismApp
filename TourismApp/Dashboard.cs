using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using static System.Net.WebRequestMethods;
using System.Reflection.Emit;
using System.Windows.Controls;

namespace TourismApp
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private static bool isEditMode = false;
        private static string editId = "";


        private static bool isEditModePlace = false;
        private static string editIdPlace = "";

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice captureDevice;

        private SQLiteDataAdapter sql_adptr;
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataReader sql_dbr;
        private string generatedSessionID;
        private int ArchiveBTNState = 0;
        private int RegisterTBTNState = 0;
        private int TouristTBTNState = 0;
        private int ArchiveFilterSet = 0;
        private string TypeFilterSet = "Local";


        // Places

        private DataSet DSPlace = new DataSet();
        private DataTable DTPlace = new DataTable();


        // Tourist
        public static string selectedTouristID;
        private DataSet DSTourist = new DataSet();
        private DataTable DTTourist = new DataTable();

        private DataSet DSVTourist = new DataSet();
        private DataTable DTVTourist = new DataTable();

        private DataSet DSV2Tourist = new DataSet();
        private DataTable DTV2Tourist = new DataTable();


        private void setConnection()
        {
            sql_con = new SQLiteConnection("Data Source = APPDB.db; Version = 3; New = False; Compress = True;");
        }


        private void Dashboard_Load(object sender, EventArgs e)
        {
            hidethisPannels();
            GenerateRandomID(8);
        }

        private void hidethisPannels()
        {
            registerdUserDetailsPanel1.Hide();
            registerdUserDetailsPanel2.Hide();
            regconfirmdetails.Hide();
            label12.Hide();
        }

        private void InserIntoDB(string txtquery)
        {
            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtquery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }


        private void GenerateRandomID(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            generatedSessionID = randomString;

        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            if (RegisterTBTNState == 0 )
            {
                RegisterButton.Text = "Update Details";
                RegisterTBTNState = 1;
            } 
            dispFName.Text = txtBoxTouristFName.Text;
            dispLName.Text = txtBoxTouristLName.Text;
            dispDob.Text = dtdob.Text;
            dispEmail.Text = txtBoxTouristEmail.Text;
            dispNationality.Text = txtBoxTouristNationality.Text;
            dispPhone.Text = txtBoxTouristPhone.Text;
            dispTid.Text = generatedSessionID;
            dispSex.Text = txtBoxTouristLSex.Text;
            dispType.Text = txtBoxTouristType.Text;

            UpdateRegisteredDetails();
           
            generateQR(generatedSessionID);
        }

        private void clearDetails()
        {


            txtBoxTouristFName.Text = "";
            txtBoxTouristLName.Text = "";
            txtBoxTouristEmail.Text = "";
            txtBoxTouristPhone.Text = "";
            txtBoxTouristType.Text = "";
            txtBoxTouristLSex.Text = "";
            txtBoxTouristNationality.Text = "";
            dtdob.Text = "";

        }

        private void UpdateRegisteredDetails()
        {

            registerdUserDetailsPanel1.Show();
            registerdUserDetailsPanel2.Show();
            regconfirmdetails.Show();
            label12.Show();
        }

        private void generateQR(string generatedId)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(generatedId, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10);

            pictureBox1.Image = qrCodeImage;
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void regconfirmdetails_Click(object sender, EventArgs e)
        {
            string txtquery = "";



            if (isEditMode)
            {
                
                txtquery = "UPDATE Tourists set FirstName  = '" + dispFName.Text + "', LastName  = '" + dispLName.Text + "',  Sex  = '" + dispSex.Text + "',  DOB  = '" + dispDob.Text + "',  Nationality  = '" + dispNationality.Text + "',  Email  = '" + dispEmail.Text + "',  Type  = '" + dispType.Text + "', DateUpdated =  '" + DateTime.Now + "'  WHERE id = '" + editId + "' ";
            } else
            {
                 txtquery = "INSERT into Tourists (id, FirstName, LastName, Sex, DOB, Nationality, Email, Phone, DateAdded, DateUpdated, Archive, Type ) values ('" + generatedSessionID + "', '" + txtBoxTouristFName.Text + "', '" + txtBoxTouristLName.Text + "', '" + txtBoxTouristLSex.Text + "',  '" + dtdob.Text + "', '" + txtBoxTouristNationality.Text + "', '" + txtBoxTouristEmail.Text + "', '" + txtBoxTouristPhone.Text + "', '" + DateTime.Now + "', '" + DateTime.Now + "', '" + 0 + "', '" + txtBoxTouristType.Text + "')";
            }
          
            InserIntoDB(txtquery);
            clearDetails();
            GenerateRandomID(8);
            hidethisPannels();////////

          

            if (isEditMode)
            {
                MessageBox.Show("User Succesfully Updated.", "Success");
            }
            else
            {
                MessageBox.Show("User Succesfully Registered.", "Success");
            }


            isEditMode = false;
        }

        private void MenuRegPlace_Click(object sender, EventArgs e)
        {
            RegisterPlaceButton.Text = "Register";
          
            DashboardTab.SelectedIndex= 1;
            DGVRegisteredPlaces.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            loadRegisteredPlacesDGV();
           
        }


        private void loadRegisteredPlacesDGV()
        {
            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();

            // Employee
            string CommandText = "SELECT id, Name, Address FROM Places";
            sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
            DSPlace.Reset();
            sql_adptr.Fill(DSPlace);
            DTPlace = DSPlace.Tables[0];
            DGVRegisteredPlaces.DataSource = DTPlace;

            sql_con.Close();
        }

        private void loadRegisteredTouristDGV()
        {
            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();

            // Employee
            string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',  Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "'";
            sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
            DSTourist.Reset();
            sql_adptr.Fill(DSTourist);
            DTTourist = DSTourist.Tables[0];
            DGVRegisteredTourist.DataSource = DTTourist;

            sql_con.Close();
        }



        private void MenuRegTourist_Click(object sender, EventArgs e)
        {
            label10.Text = "Register Tourist";
            RegisterButton.Text = "Register";
            DashboardTab.SelectedIndex = 0;
            //stopDevice();
        }

        private void RegisterPlaceButton_Click(object sender, EventArgs e)
        {
            string txtquery;
            if (isEditModePlace)
            {
                txtquery = "UPDATE Places set Name =  '" + txtBoxPlaceName.Text + "', Address =  '" + txtBoxPlaceAddress.Text + "', DateUpdated =  '" + DateTime.Now + "'   WHERE id = '" + editIdPlace + "'";
            } else
            {
                 txtquery = "INSERT into Places (Name, Address, DateAdded, DateUpdated, Archive ) values ('" + txtBoxPlaceName.Text + "', '" + txtBoxPlaceAddress.Text + "', '" + DateTime.Now + "', '" + DateTime.Now + "', '" + 0 + "')";
            }


            if (isEditModePlace)
            {

                MessageBox.Show("Edit Success", "Success");
            }
            else
            {
                MessageBox.Show("Register Success", "Success");
            }


            isEditModePlace = false;



            txtBoxPlaceName.Text = "";
            txtBoxPlaceAddress.Text = "";
            InserIntoDB(txtquery);
            loadRegisteredPlacesDGV();

            RegisterPlaceButton.Text = "Register";

        }

        private void MenuRegTouristAll_Click(object sender, EventArgs e)
        {
            DashboardTab.SelectedIndex = 2;
            DGVRegisteredTourist.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            loadRegisteredTouristDGV();
            filterHelper();
         
        }

        private void DGVRegisteredTourist_Click(object sender, EventArgs e)
        {
         
        }

        private void BTNArchiveUser_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedTouristID))
            {
                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                string txtquery;

                if (ArchiveBTNState == 0)
                {
                    txtquery = "update Tourists set Archive = 1, DateUpdated =  '" + DateTime.Now + "'   WHERE id = '" + selectedTouristID + "'";
                }
                else
                {
                    txtquery = "update Tourists set Archive = 0, DateUpdated =  '" + DateTime.Now + "'    WHERE id = '" + selectedTouristID + "'";
                }


                InserIntoDB(txtquery);
                loadRegisteredTouristDGV();
            }
            else
            {
                MessageBox.Show("Select a User first!", "Error");
            }
         

        }

        private void BTNShowArchivedUsers_Click(object sender, EventArgs e)
        {
            if  (ArchiveBTNState == 0)
            {
                
                BTNShowArchivedUsers.Text = "Show Active Users";
                BTNArchiveUser.Text = "UncArchive User";
                ArchiveFilterSet = 1;


                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Employee
                string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',  Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "' AND Type = '" + TypeFilterSet + "'";
                sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
                DSTourist.Reset();
                sql_adptr.Fill(DSTourist);
                DTTourist = DSTourist.Tables[0];
                DGVRegisteredTourist.DataSource = DTTourist;

                sql_con.Close();

                
                ArchiveBTNState = 1;
            } else
            {
                BTNShowArchivedUsers.Text = "Show Archived Users";
                BTNArchiveUser.Text = "Archive User";
                ArchiveFilterSet = 0;

                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Employee
                string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',  Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "' AND Type = '" + TypeFilterSet + "'";
                sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
                DSTourist.Reset();
                sql_adptr.Fill(DSTourist);
                DTTourist = DSTourist.Tables[0];
                DGVRegisteredTourist.DataSource = DTTourist;

                sql_con.Close();

                ArchiveBTNState = 0;
            }

            filterHelper();
        }

        private void filterHelper ()
        {
            string helperArhiveState;
            string helperFilterState;

            if (ArchiveBTNState == 0 )
            {
                helperArhiveState = "active";
            } else
            {
                helperArhiveState = "archived";
            }

            if (TouristTBTNState == 0)
            {
                helperFilterState = "users";
            }
            else if (TouristTBTNState == 1)
            {
                helperFilterState = "locals";
            }   
            else if (TouristTBTNState == 2)
            {
                helperFilterState = "tourists";
            } else
            {
                helperFilterState = "tourists";
            }

            FilterHelper.Text = "Showing  " + helperArhiveState + " " + helperFilterState + ".";
        }

        private void BTNShowTouristType_Click(object sender, EventArgs e)
        {
            if (TouristTBTNState == 0) {
                BTNShowTouristType.Text = "Tourist Only";
                TypeFilterSet = "Local";

                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Employee
                string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',   Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "' AND Type = '" + TypeFilterSet + "'";
                sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
                DSTourist.Reset();
                sql_adptr.Fill(DSTourist);
                DTTourist = DSTourist.Tables[0];
                DGVRegisteredTourist.DataSource = DTTourist;

                sql_con.Close();

                TouristTBTNState = 1;
            } else if (TouristTBTNState == 1)
            {
                BTNShowTouristType.Text = "Show All";
                TypeFilterSet = "Tourist";

                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Employee
                string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',   Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "' AND Type = '" + TypeFilterSet + "'";
                sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
                DSTourist.Reset();
                sql_adptr.Fill(DSTourist);
                DTTourist = DSTourist.Tables[0];
                DGVRegisteredTourist.DataSource = DTTourist;

                sql_con.Close();
                TouristTBTNState = 2;
            }
            else if (TouristTBTNState == 2)
            {
                BTNShowTouristType.Text = "Local Only";
               

                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Employee
                string CommandText = "SELECT id AS  'Tourist ID', FirstName AS  'First Name', LastName AS  'Last Name', Type AS 'Type',   Sex AS  'Sex', DOB AS  'Date of Birth', Nationality AS  'Nationality', Email AS  'Email', Phone AS  'Phone' FROM Tourists WHERE Archive =  '" + ArchiveFilterSet + "'";
                sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
                DSTourist.Reset();
                sql_adptr.Fill(DSTourist);
                DTTourist = DSTourist.Tables[0];
                DGVRegisteredTourist.DataSource = DTTourist;

                sql_con.Close();
                TouristTBTNState = 0;
            }
            filterHelper();
        }

        private void ViewUserDetails_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedTouristID))
            {
                ViewUserId.Text = DGVRegisteredTourist.SelectedRows[0].Cells[0].Value.ToString();
                ViewUserFname.Text = DGVRegisteredTourist.SelectedRows[0].Cells[1].Value.ToString();
                ViewUserLname.Text = DGVRegisteredTourist.SelectedRows[0].Cells[2].Value.ToString();
                ViewUserType.Text = DGVRegisteredTourist.SelectedRows[0].Cells[3].Value.ToString();
                ViewUserSex.Text = DGVRegisteredTourist.SelectedRows[0].Cells[4].Value.ToString();
                ViewUserDob.Text = DGVRegisteredTourist.SelectedRows[0].Cells[5].Value.ToString();
                ViewUserNationality.Text = DGVRegisteredTourist.SelectedRows[0].Cells[6].Value.ToString();
                ViewUserEmail.Text = DGVRegisteredTourist.SelectedRows[0].Cells[7].Value.ToString();
                ViewUserPhone.Text = DGVRegisteredTourist.SelectedRows[0].Cells[8].Value.ToString();


                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(DGVRegisteredTourist.SelectedRows[0].Cells[0].Value.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(10);

                ViewUserQR.Image = qrCodeImage;


                QueryHistoryViewDetails(DGVRegisteredTourist.SelectedRows[0].Cells[0].Value.ToString());

                DashboardTab.SelectedIndex = 3; 
            } else
            {
                MessageBox.Show("Select a User first!", "Error");
            }
            
        }

        private void QueryHistoryViewDetails(string id)
        {

            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();

            // Employee
            string CommandText = "Select p.name AS 'Place Name', p.Address as 'Place Address', h.Time as 'Time In' from Places AS p INNER JOIN History AS H On H.VisitorID = '" + id + "' AND H.PlaceID = p.id";
            sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
            DSVTourist.Reset();
            sql_adptr.Fill(DSVTourist);
            DTVTourist = DSVTourist.Tables[0];
            DGVViewDetailsHistory.DataSource = DTVTourist;

            sql_con.Close();
        }


        private void QueryHistoryViewDetails2(string id)
        {

            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();

            // Employee
            string CommandText = "Select p.name AS 'Place Name', p.Address as 'Place Address', h.Time as 'Time In' from Places AS p INNER JOIN History AS H On H.VisitorID = '" + id + "' AND H.PlaceID = p.id";
            sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
            DSV2Tourist.Reset();
            sql_adptr.Fill(DSV2Tourist);
            DTV2Tourist = DSV2Tourist.Tables[0];
            DGVViewUserDetails2.DataSource = DTV2Tourist;

            sql_con.Close();
        }


        private void MenuQRScanner_Click(object sender, EventArgs e)
        {
            BTNAllowEntry.Hide();
            DashboardTab.SelectedIndex = 4;
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                comboBox1.Items.Add(filterInfo.Name);
                comboBox1.SelectedIndex = 0;
            };

           
          
        }

        //private void LoadPlacesForQR()
        //{

        //    setConnection();
        //    sql_con.Open();
        //    sql_cmd = sql_con.CreateCommand();

        //    // Employee
        //    string CommandText = "SELECT Name AS  Name, Address AS  Address  FROM Places";
        //    sql_adptr = new SQLiteDataAdapter(CommandText, sql_con);
        //    DSPlace.Reset();
        //    sql_adptr.Fill(DSPlace);
        //    DTPlace = DSPlace.Tables[0];
        //    DGVRegisteredPlaces.DataSource = DTPlace;

        //    comboBox2 = DTPlace;
        //    comboBox2.DisplayMember = "Name";
        //    comboBox2.ValueMember = "id";

        //    sql_con.Close();


        
        //}

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void stopDevice()
        {
            if (captureDevice.IsRunning)
            {
                captureDevice.Stop();
            }
        }

        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox2.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void updateVisitorHistory(string VisitorID)
        {
            string txtquery = "INSERT into History (VisitorID, PlaceID, Time ) values ('" + VisitorID + "', '" + placeId.Text + "', '" + DateTime.Now + "')";
            InserIntoDB(txtquery);
        
        }

        private void restartQr()
        {
            captureDevice = new VideoCaptureDevice(filterInfoCollection[comboBox1.SelectedIndex].MonikerString);
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();
            timer1.Start();
        }

        private void generateQRView(string id)
        {
            
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10);

            ViewuserDetailsQR.Image = qrCodeImage;
        }


        private void queryUserData(string id)
        {
            setConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = "SELECT * FROM Tourists WHERE id =  '" + id + "'";
            sql_cmd.ExecuteNonQuery();
            sql_dbr = sql_cmd.ExecuteReader();
          
            while (sql_dbr.Read())
            {
                //authaccess = sql_dbr.GetInt32(8);
                //hostID = sql_dbr.GetString(1);
                //count = count + 1;

                ViewUserDetailsID.Text = sql_dbr.GetString(0);
                ViewUserDetailsFname.Text = sql_dbr.GetString(1);
                ViewUserDetailsLname.Text = sql_dbr.GetString(2);
                ViewUserDetailsSex.Text = sql_dbr.GetString(3);
                ViewUserDetailsDOB.Text = sql_dbr.GetString(4);
                ViewUserDetailsNationality.Text = sql_dbr.GetString(5);
                ViewUserDetailsEmail.Text = sql_dbr.GetString(6);
                ViewUserDetailsPhone.Text = sql_dbr.GetString(7);
                ViewUserDetailsType.Text = sql_dbr.GetString(10);

            }
            sql_con.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null )
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode((Bitmap)pictureBox2.Image);
                if (result != null) {
                   
                        placeId.Text = result.ToString();
                    BTNAllowEntry.Show();
                    queryUserData(placeId.Text);
                    generateQRView(placeId.Text);
                    QueryHistoryViewDetails2(placeId.Text);
                        timer1.Stop();
                        stopDevice();
                    
                 
                }

                
            }

          
        }

        private void ScanQR_Click(object sender, EventArgs e)
        {

            if (!String.IsNullOrEmpty(placeId.Text))
            {

                captureDevice = new VideoCaptureDevice(filterInfoCollection[comboBox1.SelectedIndex].MonikerString);
                captureDevice.NewFrame += CaptureDevice_NewFrame;
                captureDevice.Start();
                timer1.Start();
            }
            else
            {
                MessageBox.Show("Input Place Id First", "Error");
            }


        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            updateVisitorHistory(placeId.Text);
            BTNAllowEntry.Hide();
            ViewuserDetailsQR.Image = null; ;

            ViewUserDetailsID.Text = "Waiting for Data";
            ViewUserDetailsFname.Text = "Waiting for Data";
            ViewUserDetailsLname.Text = "Waiting for Data";
            ViewUserDetailsSex.Text = "Waiting for Data";
            ViewUserDetailsDOB.Text = "Waiting for Data";
            ViewUserDetailsNationality.Text = "Waiting for Data";
            ViewUserDetailsEmail.Text = "Waiting for Data";
            ViewUserDetailsPhone.Text = "Waiting for Data";
            ViewUserDetailsType.Text = "Waiting for Data";
        }

        private void button3_Click_2(object sender, EventArgs e)
        {

            label10.Text = "Edit Tourist Details";
        
            RegisterButton.Text = "Update";

            txtBoxTouristFName.Text = ViewUserFname.Text;
            txtBoxTouristLName.Text = ViewUserLname.Text;
            txtBoxTouristEmail.Text = ViewUserEmail.Text;
            txtBoxTouristPhone.Text = ViewUserPhone.Text;
            txtBoxTouristType.Text = ViewUserType.Text;
            txtBoxTouristLSex.Text = ViewUserSex.Text;
            txtBoxTouristNationality.Text = ViewUserNationality.Text;
            dtdob.Text = ViewUserDob.Text;

            editId = ViewUserId.Text;
            isEditMode = true;


          



            DashboardTab.SelectedIndex = 0;
        }

        private void delUser_Click(object sender, EventArgs e)
        {
            string txtquery;

            if (!String.IsNullOrEmpty(selectedTouristID))
            {
                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();


              if (MessageBox.Show("Please confirm before proceed" + "\n" + "Do you want to Continue ?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    txtquery = "DELETE FROM Tourists WHERE id = '" + selectedTouristID + "'";
                }
                else
                {
                    txtquery = "";
                }
               
                InserIntoDB(txtquery);
                loadRegisteredTouristDGV();
            }
            else
            {
                MessageBox.Show("Select a User first!", "Error");
            }
        }

        private void edPlace_Click(object sender, EventArgs e)
        {
            isEditModePlace = true;

            if (!String.IsNullOrEmpty(editIdPlace))
            {

                txtBoxPlaceName.Text = DGVRegisteredPlaces.SelectedRows[0].Cells[1].Value.ToString();
                txtBoxPlaceAddress.Text = DGVRegisteredPlaces.SelectedRows[0].Cells[2].Value.ToString();

                editIdPlace = DGVRegisteredPlaces.SelectedRows[0].Cells[0].Value.ToString();

                RegisterPlaceButton.Text = "Update";
            }
            else
            {
                MessageBox.Show("Select a Place first!", "Error");
            }


        }

        private void delPlace_Click(object sender, EventArgs e)
        {
            string txtquery;

            if (!String.IsNullOrEmpty(editIdPlace))
            {
                setConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();


                if (MessageBox.Show("Please confirm before proceed" + "\n" + "Do you want to Continue ?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    txtquery = "DELETE FROM Places WHERE id = '" + editIdPlace + "'";
                }
                else
                {
                    txtquery = "";
                }

                InserIntoDB(txtquery);
                loadRegisteredPlacesDGV();
            }
            else
            {
                MessageBox.Show("Select a Place first!", "Error");
            }
        }

        private void DGVRegisteredPlaces_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            editIdPlace = DGVRegisteredPlaces.SelectedRows[0].Cells[0].Value.ToString();
        }

        private void DGVRegisteredPlaces_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            editIdPlace = DGVRegisteredPlaces.SelectedRows[0].Cells[0].Value.ToString();
        }

        private void DGVRegisteredTourist_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedTouristID = DGVRegisteredTourist.SelectedRows[0].Cells[0].Value.ToString();
        }

        private void DGVRegisteredTourist_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedTouristID = DGVRegisteredTourist.SelectedRows[0].Cells[0].Value.ToString();
        }
    }
}
