
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;

using HerboldRacing;

namespace IRSDKSharperTest
{
	public partial class MainWindow : Window
	{
		private int lastTickCount = -1;

		private float[] carIdxLapDistPct = new float[ 64 ];
		private int lapsCompleted = 0;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void UpdateValueLabels()
		{
			var irsdk = Program.IRSDKSharper;

			irsdkValueLabel.Content = ( irsdk == null ) ? "irsdk = null" : "irsdk = not null";

			if ( irsdk == null )
			{
				irsdkIsStartedValueLabel.Content = "irsdk.IsStarted = ?";
				irsdkIsConnectedValueLabel.Content = "irsdk.IsConnected = ?";
				irsdkUpdateIntervalValueLabel.Content = "irsdk.UpdateInterval = ?";
			}
			else
			{
				irsdkIsStartedValueLabel.Content = $"irsdk.IsStarted = {irsdk.IsStarted}";
				irsdkIsConnectedValueLabel.Content = $"irsdk.IsConnected = {irsdk.IsConnected}";
				irsdkUpdateIntervalValueLabel.Content = $"irsdk.UpdateInterval = {irsdk.UpdateInterval}";
			}

			var renderTimeInMilliseconds = dataView.RenderTime * 1000;
			var renderTimeInFPS = ( dataView.RenderTime > 0 ) ? 1 / dataView.RenderTime : 0;

			renderTime.Content = $"renderTime = {renderTimeInMilliseconds:0.00} ms ({renderTimeInFPS:0} FPS)";
		}

		private void OnException( Exception exception )
		{
			var irsdk = Program.IRSDKSharper;

			irsdk?.Stop();

			Dispatcher.BeginInvoke( () =>
			{
				MessageBox.Show( exception.Message, "OnException()", MessageBoxButton.OK, MessageBoxImage.Exclamation );
			} );
		}

		private void OnConnected()
		{
			Dispatcher.BeginInvoke( () =>
			{
				UpdateValueLabels();
			} );
		}

		private void OnDisconnected()
		{
			Dispatcher.BeginInvoke( () =>
			{
				UpdateValueLabels();

				dataView.InvalidateVisual();

				scrollBar.Maximum = 1;
			} );
		}

		private void OnSessionInfo()
		{
			RedrawWindow();

			var irsdk = Program.IRSDKSharper;

			if ( false && ( irsdk != null ) )
			{
				var desktopFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );

				var filePath = $"{desktopFolderPath}\\IRSDKSharperTest\\{irsdk.Data.SessionInfo.WeekendInfo.SubSessionID}_{irsdk.Data.SessionInfoUpdate}.yaml";

				File.WriteAllText( filePath, irsdk.Data.SessionInfoYaml );
			}
		}

		private void OnTelemetryData()
		{
			var irsdk = Program.IRSDKSharper;

			if ( irsdk != null )
			{
				irsdk.Data.GetFloatArray( "CarIdxLapDistPct", carIdxLapDistPct, 0, carIdxLapDistPct.Length );
			}

			RedrawWindow();
		}
		private void CaptureRaceProperties()
		{
            var irsdk = Program.IRSDKSharper;

			if (irsdk != null)
			{
				Object LastLapNLapTime = irsdk.Data.GetValue("LapLastNLapTime");
				Object LastLapTime = irsdk.Data.GetValue("LapLastLapTime");
				Object FuelLevel = irsdk.Data.GetValue("FuelLevel");
				Object SessionTime = irsdk.Data.GetValue("SessionTime");
				Object SessionTimeRemain = irsdk.Data.GetValue("SessionTimeRemain");
				Object SessionTimeTotal = irsdk.Data.GetValue("SessionTimeTotal");
				Object SessionLapsRemainEx = irsdk.Data.GetValue("SessionLapsRemainEx");
				Object SessionLapsRemain = irsdk.Data.GetValue("SessionLapsRemain");
				Object LapCompleted	= irsdk.Data.GetValue("LapCompleted");
				Object RaceLaps = irsdk.Data.GetValue("RaceLaps");
				Object SessionLapsTotal = irsdk.Data.GetValue("SessionLapsTotal");

				Debug.WriteLine("");
				Debug.WriteLine("LapLastNLapTime: " + LastLapNLapTime);
				Debug.WriteLine("LapLastLapTime: " + LastLapTime);
				Debug.WriteLine("FuelLevel: " + FuelLevel);
				Debug.WriteLine("SessionTime: " + SessionTime);
				Debug.WriteLine("SessionTimeRemain: " + SessionTimeRemain);
				Debug.WriteLine("SessionTimeTotal: " + SessionTimeTotal);
				Debug.WriteLine("SessionLapsRemainEx: " + SessionLapsRemainEx);
				Debug.WriteLine("SessionLapsRemain: " + SessionLapsRemain);
				Debug.WriteLine("LapCompleted: " + LapCompleted);
				Debug.WriteLine("RaceLaps: " + RaceLaps);
				Debug.WriteLine("SessionLapsTotal: " + SessionLapsTotal); 
				Debug.WriteLine("-----------------------------------");

				if (lapsCompleted < (int)LapCompleted)
				{
                    Debug.WriteLine("Lap Completed: " + LapCompleted);
                }

				lapsCompleted = (int)LapCompleted;

			}
        }

		private void OnStopped()
		{
			lastTickCount = -1;

			Dispatcher.BeginInvoke( () =>
			{
				UpdateValueLabels();

				dataView.InvalidateVisual();

				scrollBar.Maximum = 1;
			} );
		}

		private void RedrawWindow()
		{
			var irsdk = Program.IRSDKSharper;

			if ( ( irsdk != null ) && ( irsdk.Data.TickCount != lastTickCount ) )
			{
				lastTickCount = irsdk.Data.TickCount;

				Dispatcher.BeginInvoke( () =>
				{
					UpdateValueLabels();

					dataView.InvalidateVisual();

					scrollBar.Maximum = dataView.NumLines - 1;
				} );
			}
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			var irsdk = Program.IRSDKSharper;

			irsdk?.Stop();
		}

		private void Window_MouseWheel( object sender, System.Windows.Input.MouseWheelEventArgs e )
		{
			var delta = e.Delta / 30.0f;

			if ( delta > 0 )
			{
				delta = Math.Max( 1, delta );
			}
			else
			{
				delta = Math.Min( -1, delta );
			}

			scrollBar.Value -= delta;

			dataView.SetScrollIndex( (int) scrollBar.Value );
		}

		private void ScrollBar_Scroll( object sender, ScrollEventArgs e )
		{
			dataView.SetScrollIndex( (int) e.NewValue );
		}

		private void HeaderDataButton_Click( object sender, RoutedEventArgs e )
		{
			dataView.SetMode( 0 );
		}

		private void SessionInfoButton_Click( object sender, RoutedEventArgs e )
		{
			dataView.SetMode( 1 );
		}

		private void TelemetryDataButton_Click( object sender, RoutedEventArgs e )
		{
			dataView.SetMode( 2 );
		}

		private void Create_Click( object sender, RoutedEventArgs e )
		{
			Program.IRSDKSharper = new IRSDKSharper();

			var irsdk = Program.IRSDKSharper;

			irsdk.OnException += OnException;
			irsdk.OnConnected += OnConnected;
			irsdk.OnDisconnected += OnDisconnected;
			irsdk.OnSessionInfo += OnSessionInfo;
			irsdk.OnTelemetryData += OnTelemetryData;
			irsdk.OnTelemetryData += CaptureRaceProperties;
			irsdk.OnStopped += OnStopped;

			var desktopFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );

			irsdk.EnableEventSystem( $"{desktopFolderPath}\\IRSDKSharperTest" );

			UpdateValueLabels();
		}

		private void Start_Click( object sender, RoutedEventArgs e )
		{
			var irsdk = Program.IRSDKSharper;
			Debug.WriteLine("Start_Click");

			if ( irsdk != null )
			{
                irsdk.Start();
                irsdk.UpdateInterval = 300;
                lapsCompleted = 0;
                UpdateValueLabels();
            }
		}

		private void Stop_Click( object sender, RoutedEventArgs e )
		{
			var irsdk = Program.IRSDKSharper;

			irsdk?.Stop();

			UpdateValueLabels();
		}

		private void IncrementUpdateInterval_Click( object sender, RoutedEventArgs e )
		{
			var irsdk = Program.IRSDKSharper;

			if ( irsdk != null )
			{
				irsdk.UpdateInterval++;

				UpdateValueLabels();
			}
		}

		private void DecrementUpdateInterval_Click( object sender, RoutedEventArgs e )
		{
			var irsdk = Program.IRSDKSharper;

			if ( irsdk != null )
			{
				irsdk.UpdateInterval--;

				UpdateValueLabels();
			}
		}

		private void Dispose_Click( object sender, RoutedEventArgs e )
		{
			Program.IRSDKSharper = null;

			UpdateValueLabels();
		}
	}
}
