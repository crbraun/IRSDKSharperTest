﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using HerboldRacing;

namespace IRSDKSharperTest
{
	public class DataViewer : Control
	{
		public int NumLines { get; private set; } = 0;

		private IRacingSdkData? iRacingSdkData = null;
		private int scrollIndex = 0;
		private int mode = 1;

		private readonly CultureInfo cultureInfo = CultureInfo.GetCultureInfo( "en-us" );
		private readonly Typeface typeface = new( "Courier New" );

		static DataViewer()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( DataViewer ), new FrameworkPropertyMetadata( typeof( DataViewer ) ) );
		}

		public void SetIRacingSdkData( IRacingSdkData? iRacingSdkData )
		{
			this.iRacingSdkData = iRacingSdkData;
		}

		public void SetScrollIndex( int scrollIndex )
		{
			this.scrollIndex = scrollIndex;
		}

		public void SetMode( int mode )
		{
			this.mode = mode;
		}

		protected override void OnRender( DrawingContext drawingContext )
		{
			base.OnRender( drawingContext );

			switch ( mode )
			{
				case 0: DrawHeaderData( drawingContext ); break;
				case 1: DrawTelemetryData( drawingContext ); break;
				case 2: DrawSessionInfo( drawingContext ); break;
			}
		}

		private void DrawHeaderData( DrawingContext drawingContext )
		{
			if ( iRacingSdkData != null )
			{
				var dictionary = new Dictionary<string, int>()
				{
					{ "Version", iRacingSdkData.Version },
					{ "Status", iRacingSdkData.Status },
					{ "TickRate", iRacingSdkData.TickRate },
					{ "SessionInfoUpdate", iRacingSdkData.SessionInfoUpdate },
					{ "SessionInfoLength", iRacingSdkData.SessionInfoLength },
					{ "SessionInfoOffset", iRacingSdkData.SessionInfoOffset },
					{ "VarCount", iRacingSdkData.VarCount },
					{ "VarHeaderOffset", iRacingSdkData.VarHeaderOffset },
					{ "BufferCount", iRacingSdkData.BufferCount },
					{ "BufferLength", iRacingSdkData.BufferLength },
					{ "TickCount", iRacingSdkData.TickCount },
					{ "Offset", iRacingSdkData.Offset },
					{ "FramesDropped", iRacingSdkData.FramesDropped }
				};

				var point = new Point( 10, 10 );
				var lineIndex = 0;

				foreach ( var keyValuePair in dictionary )
				{
					if ( ( lineIndex & 1 ) == 1 )
					{
						drawingContext.DrawRectangle( Brushes.AliceBlue, null, new Rect( 0, point.Y, ActualWidth, 20 ) );
					}

					var formattedText = new FormattedText( keyValuePair.Key, cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
					{
						LineHeight = 20
					};

					drawingContext.DrawText( formattedText, point );

					point.X += 150;

					formattedText = new FormattedText( keyValuePair.Value.ToString(), cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
					{
						LineHeight = 20
					};

					drawingContext.DrawText( formattedText, point );

					point.X = 10;
					point.Y += 20;

					lineIndex++;
				}

				NumLines = lineIndex;
			}
			else
			{
				drawingContext.DrawRectangle( Brushes.DarkGray, null, new Rect( 0, 0, ActualWidth, ActualHeight ) );
			}
		}

		private void DrawTelemetryData( DrawingContext drawingContext )
		{
			if ( ( iRacingSdkData != null ) && ( iRacingSdkData.TelemetryData != null ) )
			{
				var point = new Point( 10, 10 );
				var lineIndex = 0;
				var stopDrawing = false;

				foreach ( var keyValuePair in iRacingSdkData.TelemetryData )
				{
					for ( var valueIndex = 0; valueIndex < keyValuePair.Value.Count; valueIndex++ )
					{
						if ( ( lineIndex >= scrollIndex ) && !stopDrawing )
						{
							if ( ( lineIndex & 1 ) == 1 )
							{
								drawingContext.DrawRectangle( Brushes.AliceBlue, null, new Rect( 0, point.Y, ActualWidth, 20 ) );
							}

							var formattedText = new FormattedText( keyValuePair.Value.Offset.ToString(), cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
							{
								LineHeight = 20
							};

							drawingContext.DrawText( formattedText, point );

							point.X += 40;

							formattedText = new FormattedText( keyValuePair.Value.Name, cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
							{
								LineHeight = 20
							};

							drawingContext.DrawText( formattedText, point );

							point.X += 230;

							if ( keyValuePair.Value.Count > 1 )
							{
								formattedText = new FormattedText( valueIndex.ToString(), cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
								{
									LineHeight = 20
								};

								drawingContext.DrawText( formattedText, point );
							}

							point.X += 30;

							var valueAsString = string.Empty;
							var bitsAsString = string.Empty;
							var brush = Brushes.Black;

							switch ( keyValuePair.Value.Unit )
							{
								case "irsdk_TrkLoc":
									valueAsString = GetString<IRacingSdkEnum.TrkLoc>( keyValuePair.Value, valueIndex );
									break;

								case "irsdk_TrkSurf":
									valueAsString = GetString<IRacingSdkEnum.TrkSurf>( keyValuePair.Value, valueIndex );
									break;

								case "irsdk_SessionState":
									valueAsString = GetString<IRacingSdkEnum.SessionState>( keyValuePair.Value, valueIndex );
									break;

								case "irsdk_CarLeftRight":
									valueAsString = GetString<IRacingSdkEnum.CarLeftRight>( keyValuePair.Value, valueIndex );
									break;

								case "irsdk_PitSvStatus":
									valueAsString = GetString<IRacingSdkEnum.PitSvStatus>( keyValuePair.Value, valueIndex );
									break;

								case "irsdk_PaceMode":
									valueAsString = GetString<IRacingSdkEnum.PaceMode>( keyValuePair.Value, valueIndex );
									break;

								default:

									switch ( keyValuePair.Value.VarType )
									{
										case IRacingSdkEnum.VarType.Char:
											valueAsString = $"         {iRacingSdkData.GetChar( keyValuePair.Value.Name, valueIndex )}";
											break;

										case IRacingSdkEnum.VarType.Bool:
											var valueAsBool = iRacingSdkData.GetBool( keyValuePair.Value.Name, valueIndex );
											valueAsString = valueAsBool ? "         T" : "         F";
											brush = valueAsBool ? Brushes.Green : Brushes.Red;
											break;

										case IRacingSdkEnum.VarType.Int:
											valueAsString = $"{iRacingSdkData.GetInt( keyValuePair.Value.Name, valueIndex ),10:N0}";
											break;

										case IRacingSdkEnum.VarType.BitField:
											valueAsString = $"0x{iRacingSdkData.GetBitField( keyValuePair.Value.Name, valueIndex ):X8}";

											switch ( keyValuePair.Value.Unit )
											{
												case "irsdk_EngineWarnings":
													bitsAsString = GetString<IRacingSdkEnum.EngineWarnings>( keyValuePair.Value, valueIndex );
													break;

												case "irsdk_Flags":
													bitsAsString = GetString<IRacingSdkEnum.Flags>( keyValuePair.Value, valueIndex );
													break;

												case "irsdk_CameraState":
													bitsAsString = GetString<IRacingSdkEnum.CameraState>( keyValuePair.Value, valueIndex );
													break;

												case "irsdk_PitSvFlags":
													bitsAsString = GetString<IRacingSdkEnum.PitSvFlags>( keyValuePair.Value, valueIndex );
													break;

												case "irsdk_PaceFlags":
													bitsAsString = GetString<IRacingSdkEnum.PaceFlags>( keyValuePair.Value, valueIndex );
													break;
											}

											break;

										case IRacingSdkEnum.VarType.Float:
											valueAsString = $"{iRacingSdkData.GetFloat( keyValuePair.Value.Name, valueIndex ),15:N4}";
											break;

										case IRacingSdkEnum.VarType.Double:
											valueAsString = $"{iRacingSdkData.GetDouble( keyValuePair.Value.Name, valueIndex ),15:N4}";
											break;
									}

									break;
							}

							formattedText = new FormattedText( valueAsString, cultureInfo, FlowDirection.LeftToRight, typeface, 12, brush, 1.25f )
							{
								LineHeight = 20
							};

							drawingContext.DrawText( formattedText, point );

							point.X += 150;

							formattedText = new FormattedText( keyValuePair.Value.Unit, cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
							{
								LineHeight = 20
							};

							drawingContext.DrawText( formattedText, point );

							point.X += 160;

							var desc = keyValuePair.Value.Desc;
							var originalDescLength = desc.Length;

							if ( bitsAsString != string.Empty )
							{
								desc += $" ({bitsAsString})";
							}

							formattedText = new FormattedText( desc, cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
							{
								LineHeight = 20
							};

							if ( bitsAsString != string.Empty )
							{
								formattedText.SetForegroundBrush( Brushes.OrangeRed, originalDescLength, desc.Length - originalDescLength );
							}

							drawingContext.DrawText( formattedText, point );

							point.X = 10;
							point.Y += 20;

							if ( ( point.Y + 20 ) > ActualHeight )
							{
								stopDrawing = true;
							}
						}

						lineIndex++;
					}
				}

				NumLines = lineIndex;
			}
			else
			{
				drawingContext.DrawRectangle( Brushes.DarkGray, null, new Rect( 0, 0, ActualWidth, ActualHeight ) );
			}
		}

		private void DrawSessionInfo( DrawingContext drawingContext )
		{
			if ( ( iRacingSdkData != null ) && ( iRacingSdkData.SessionInfo != null ) )
			{
				var point = new Point( 10, 10 );
				var lineIndex = 0;
				var stopDrawing = false;

				foreach ( var propertyInfo in iRacingSdkData.SessionInfo.GetType().GetProperties() )
				{
					DrawSessionInfo( drawingContext, propertyInfo.Name, propertyInfo.GetValue( iRacingSdkData.SessionInfo ), 0, ref point, ref lineIndex, ref stopDrawing );
				}

				NumLines = lineIndex;
			}
			else
			{
				drawingContext.DrawRectangle( Brushes.DarkGray, null, new Rect( 0, 0, ActualWidth, ActualHeight ) );
			}
		}

		private void DrawSessionInfo( DrawingContext drawingContext, string propertyName, object? valueAsObject, int indent, ref Point point, ref int lineIndex, ref bool stopDrawing )
		{
			var isSimpleValue = ( ( valueAsObject is null ) || ( valueAsObject is string ) || ( valueAsObject is int ) || ( valueAsObject is float ) || ( valueAsObject is double ) );

			if ( ( lineIndex >= scrollIndex ) && !stopDrawing )
			{
				if ( ( lineIndex & 1 ) == 1 )
				{
					drawingContext.DrawRectangle( Brushes.AliceBlue, null, new Rect( 0, point.Y, ActualWidth, 20 ) );
				}

				point.X = 10 + indent * 50;

				var formattedText = new FormattedText( propertyName, cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
				{
					LineHeight = 20
				};

				drawingContext.DrawText( formattedText, point );

				if ( valueAsObject is null )
				{
					point.X = 260 + indent * 50;

					formattedText = new FormattedText( "(null)", cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
					{
						LineHeight = 20
					};

					drawingContext.DrawText( formattedText, point );
				}
				else if ( isSimpleValue )
				{
					point.X = 260 + indent * 50;

					formattedText = new FormattedText( valueAsObject.ToString(), cultureInfo, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, 1.25f )
					{
						LineHeight = 20
					};

					drawingContext.DrawText( formattedText, point );
				}

				point.Y += 20;

				if ( ( point.Y + 20 ) > ActualHeight )
				{
					stopDrawing = true;
				}
			}

			lineIndex++;

			if ( !isSimpleValue )
			{
				if ( valueAsObject is IList list )
				{
					var index = 0;

					foreach ( var item in list )
					{
						DrawSessionInfo( drawingContext, index.ToString(), item, indent + 1, ref point, ref lineIndex, ref stopDrawing );

						index++;
					}
				}
				else
				{
#pragma warning disable CS8602
					foreach ( var propertyInfo in valueAsObject.GetType().GetProperties() )
					{
						DrawSessionInfo( drawingContext, propertyInfo.Name, propertyInfo.GetValue( valueAsObject ), indent + 1, ref point, ref lineIndex, ref stopDrawing );
					}
#pragma warning restore CS8602
				}
			}
		}

#pragma warning disable CS8602
		private string GetString<T>( IRacingSdkDatum var, int index ) where T : Enum
		{
			if ( var.VarType == IRacingSdkEnum.VarType.Int )
			{
				var enumValue = (T) (object) iRacingSdkData.GetInt( var.Name, index );

				return enumValue.ToString();
			}
			else
			{
				var bits = iRacingSdkData.GetBitField( var.Name, index );

				var bitsString = string.Empty;

				foreach ( uint bitMask in Enum.GetValues( typeof( T ) ) )
				{
					if ( ( bits & bitMask ) != 0 )
					{
						if ( bitsString != string.Empty )
						{
							bitsString += " | ";
						}

						bitsString += Enum.GetName( typeof( T ), bitMask );
					}
				}

				return bitsString;
			}
		}
#pragma warning restore CS8602
	}
}