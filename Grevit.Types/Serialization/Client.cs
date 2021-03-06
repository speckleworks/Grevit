﻿//
//  Grevit - Create Autodesk Revit (R) Models in McNeel's Rhino Grassopper 3D (R)
//  For more Information visit grevit.net or food4rhino.com/project/grevit
//  Copyright (C) 2015
//  Authors: Maximilian Thumfart,
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;
using System.Threading.Tasks;

namespace Grevit.Serialization
{
    public partial class Client : Form
    {
        #region Delegates

        delegate void SetTextCallback(string text);

        delegate void SetComponentsCallback(Grevit.Types.ComponentCollection comps);

        #endregion

        #region Fields

        // FamilyCollection
        private string FamiliesToRespond = string.Empty;
        
        // tcp client callback
        private AsyncCallback tcpClientCallback;
        
        // ComponentCollection for deserialized components (Result)
        public Grevit.Types.ComponentCollection componentCollection;

        // TCPListener
        private TcpListener listener;

        // Indicator to stop listening
        private bool StopListening = false;

        // Port to listen at
        private int Port = 8002;

        // IpAdress to listen at
        private IPAddress ListenIPAddress = IPAddress.Any;

        #endregion

        #region Constructors

        public Client()
        {
            InitializeComponent();
        }

        public Client(RevitFamilyCollection FamiliesToRespond)
        {
            InitializeComponent();

            // Prepare the Revit Familytree of the current document for sending
            this.FamiliesToRespond = Utilities.Serialize(FamiliesToRespond);
        }

        #endregion





        /// <summary>
        /// Load Client Window
        /// </summary>
        private void GrevitClient_Load(object sender, EventArgs e)
        {
            // Start waiting animation using ImageAnimator class
            AnimateImage();
            ImageAnimator.UpdateFrames();

            // Listen for incoming connections
            ListenForIncomingConnections();
        }


        #region waitingGifAnimation

        // indicator for running animation
        bool currentlyAnimating = false;

        /// <summary>
        /// Animate gif animation from resources
        /// </summary>
        public void AnimateImage()
        {
            if (!currentlyAnimating)
            {
                // use the image animator to invoke an animation
                ImageAnimator.Animate(Properties.Resources.waitingAnimation, new EventHandler(this.OnFrameChanged));
                currentlyAnimating = true;
            }
        }

        /// <summary>
        /// Invalidate frames when being changed
        /// </summary>
        private void OnFrameChanged(object o, EventArgs e)
        {
            this.Invalidate();
        }

        #endregion


        // Deserialze Component strings
        private static Grevit.Types.ComponentCollection StringToComponents(string dataxml)
        {
            // Deserialize xml data to an object
            object o = Utilities.Deserialize(dataxml, typeof(Grevit.Types.ComponentCollection));

            // Cast the object to a component collection and return it
            Grevit.Types.ComponentCollection cs = (Grevit.Types.ComponentCollection)o;
            
            return cs;
        }




        // Invoke an add string to listbox for debugging messages
        private void AppendDebugListMessage(string text)
        {
            if (this.debugTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AppendDebugListMessage);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.debugTextBox.Items.Add(text);
            }
        }

        // Invokable method to set received component collections to the static component collection
        private void AddComponents(Grevit.Types.ComponentCollection componentCollection)
        {
            if (this.debugTextBox.InvokeRequired)
            {
                SetComponentsCallback d = new SetComponentsCallback(AddComponents);
                this.Invoke(d, new object[] { componentCollection });
            }
            else
            {
                this.componentCollection = componentCollection;
            }
        }



        
        /// <summary>
        /// Listen for incoming connections from Grasshopper
        /// </summary>
        void ListenForIncomingConnections()
        {
            // Initialize a new component collection for the result
            componentCollection = new Grevit.Types.ComponentCollection();

            // Initialize the listener using IPAddress and Port
            listener = new TcpListener(new IPEndPoint(ListenIPAddress, Port));
            try
            {
                // Start the listener
                listener.Start();

                // Initialize the tcp callback
                tcpClientCallback = new AsyncCallback(ConnectCallback);

                // Begin to accept tcp connections (async)
                AcceptConnectionsAysnc(listener);
            }
            catch (SocketException e)
            { 
                //unhandled Exception in case anything goes wrong
            }
           
            // Write debug messages to the client screen
            AppendDebugListMessage("Client started.");
            AppendDebugListMessage("Waiting for connections.");
        }

        
        // Begin accepting connections async
        private void AcceptConnectionsAysnc(TcpListener listener)
        {
            // If listening indicator is true accept any connection triggering the connectCallback method
            if (!StopListening) listener.BeginAcceptTcpClient(tcpClientCallback, listener);
        }

        // Async Connect callback to handle incoming accepted connections
        private void ConnectCallback(IAsyncResult result)
        {
            // Write debug message to screen
            AppendDebugListMessage("Received connection request.");

            // Handle the incoming connection in the HandleReceive method as an own task
            listener = (TcpListener)result.AsyncState;
            System.Threading.Tasks.Task recieveTask = new Task(() => { HandleRecieve(result, listener); });
            
            // run the receiver task
            recieveTask.Start();
          
            // continue to accept incoming connections
            AcceptConnectionsAysnc(listener);
        }

        /// <summary>
        /// Handle an incoming connection
        /// </summary>
        private void HandleRecieve(IAsyncResult result, TcpListener listener)
        {
            // Print debug message waiting for data
            AppendDebugListMessage("Waiting for data.");

            // Interrupt this process once stop listenening is true
            if (StopListening) return;

                // Accept connection
                using (TcpClient client = listener.EndAcceptTcpClient(result))
                {
                    client.NoDelay = true;

                    // Get the data stream ftom the tcp client
                    using (NetworkStream stream = client.GetStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.AutoFlush = true;

                                // read data from sender
                                string data = reader.ReadLine();

                                // write the line to the console
                                Console.WriteLine(data);

                                // Send the received data back to the sender
                                // in Order to make sure we recieved the
                                // right amount and correct data.
                                writer.WriteLine(data);

                                // Send our Categories, Families and Types to Grasshopper
                                writer.WriteLine(FamiliesToRespond);

                                // if there is some useful data
                                if (data != null && data.Length > 5)
                                {
                                    // Set the debugger message
                                    AppendDebugListMessage("Received " + data.Length + " Bytes.");

                                    // Deserialize the received data string
                                    Grevit.Types.ComponentCollection cc = StringToComponents(data);

                                    // If anything useful has been deserialized
                                    if (cc != null && cc.Items.Count > 0)
                                    {
                                        // Add Component collection to the client static component collection
                                        AddComponents(cc);

                                        // print another debug message about the component received
                                        AppendDebugListMessage("Translated " + cc.Items.Count.ToString() + " components.");

                                        // set stop listening switch
                                        StopListening = true;
                                    }

                                }

                            }

                        }
                    }


                }

            // If stop listening is set stop all listening processes
            if (StopListening)
            {
                // Stop the listener
                listener.Server.Close();
                listener.Stop();

                // Wait until processes are stopped
                Thread.Sleep(2000);

                // close the dialog automatically
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }

        }




    }


}
