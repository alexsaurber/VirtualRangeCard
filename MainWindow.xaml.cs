﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace VirtualRangeCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
        }

        private void guntypebox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem GunTypeSel = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (guntypebox.SelectedIndex != -1)
            {
                comboxtext.Text = GunTypeSel.Tag.ToString();
            }
            else { comboxtext.Text = String.Empty; }
        }

        private void calculatebutton_Click(object sender, RoutedEventArgs e)
        {
            string[] firingSolution = findArtillerySolution();
            solutionCharge.Text = firingSolution[0];
            solutionAzimuth.Text = firingSolution[1];
            solutionElevation.Text = firingSolution[2];
            solutionTime.Text = firingSolution[3];
        }

        private void adjustfirebutton_Click(object sender, RoutedEventArgs e)
        {
            string[] firingSolution = adjustArtillerySolution();
            solutionCharge.Text = firingSolution[0];
            solutionAzimuth.Text = firingSolution[1];
            solutionElevation.Text = firingSolution[2];
            solutionTime.Text = firingSolution[3];
        }

        

        private string[] findArtillerySolution()
        {
            // set initial return values to empty/0
            string AzimuthMILS_str = String.Empty;
            string Charge_str = String.Empty;
            string Elevation_str = String.Empty;
            string FlightTime_str = String.Empty;

            // get the range to the target to calculate elevation. initially set to -1 so we can error check later
            int range = -1;
            int deltaHeight = -1;

            // convert gun input to 5 digit grid for each direciton (10 digit total grid read-out)
            while (gungridWE.Text.Length < 5) { gungridWE.Text += "0"; };
            while (gungridNS.Text.Length < 5) { gungridNS.Text += "0"; };

            // get our starting position turned into numbers
            if (Int32.TryParse(gungridWE.Text, out int startX) && Int32.TryParse(gungridNS.Text, out int startY) && Int32.TryParse(gungridASL.Text, out int startASL))
            {
                // if we are given the target grid
                //if (targetgrid.IsSelected == true)
                //{

                    // convert target input to 5 digit grid for each direciton (10 digit total grid read-out). 10 digit grids give 1 m X 1 m accuracy.
                    while (targetgridWE.Text.Length < 5) { targetgridWE.Text += "0"; };
                    while (targetgridNS.Text.Length < 5) { targetgridNS.Text += "0"; };

                    // get our termination position turned into numbers
                    if (Int32.TryParse(targetgridWE.Text, out int termX) && Int32.TryParse(targetgridNS.Text, out int termY) && Int32.TryParse(targetgridASL.Text, out int termASL))
                    {
                        // find distance in the X and Y from start to termination
                        int deltaX = termX - startX;
                        int deltaY = termY - startY;

                        // find height difference between target and gun
                        deltaHeight = termASL - startASL;

                        // get magnitude of range from gun to term so we can calculate elevation later
                        range = Convert.ToInt32(Math.Sqrt(Math.Pow(Math.Abs(deltaX), 2) + Math.Pow(Math.Abs(deltaY), 2)));
                        //MessageBox.Show(range.ToString());

                        // find angle from start to term in (converted from radians to) degrees, then orient the mathematics to correct for North
                        double angleXY = (Math.Atan2(Convert.ToDouble(deltaY), Convert.ToDouble(deltaX)) * (180 / Math.PI));
                        double bearingXY = math2comp(angleXY);

                        // convert target bearing from angle degrees to mils
                        int AzimuthMILS = Convert.ToInt32(bearingXY / 360 * 6400);
                        AzimuthMILS_str = AzimuthMILS.ToString();
                        while (AzimuthMILS_str.Length < 4) { AzimuthMILS_str = "0" + AzimuthMILS_str; };
                    }
                    else
                    {
                        MessageBox.Show("Invalid termination grid.");
                    }
                //}
                // if we are given the spotter grid
                /*else if (spottergrid.IsSelected == true)
                {
                    // convert target input to 5 digit grid for each direciton (10 digit total grid read-out)
                    while (spottergridWE.Text.Length < 5) { spottergridWE.Text += "0"; };
                    while (spottergridNS.Text.Length < 5) { spottergridNS.Text += "0"; };

                    // get our spotter position and target reference turned into numbers - we are not concerned with target height while calculating azimuth
                    if (Int32.TryParse(spottergridWE.Text, out int midX) && Int32.TryParse(spottergridNS.Text, out int midY) && Double.TryParse(targetBearing.Text, out double tgtBearing)
                        && Int32.TryParse(targetRange.Text, out int tgtRange) && Int32.TryParse(targetASL.Text, out int termASL))
                    {
                        // get grid location of termination point. convert bearing to radians, then calculate target position from spotter position
                        double tgtAbsAngle = comp2math(tgtBearing);

                        int termX = Convert.ToInt32((midX + tgtRange * Math.Cos((tgtAbsAngle) / 180 * Math.PI)));
                        int termY = Convert.ToInt32((midY + tgtRange * Math.Sin((tgtAbsAngle) / 180 * Math.PI)));

                        // find distance in the X and Y from start to termination
                        int deltaX = termX - startX;
                        int deltaY = termY - startY;

                        // find height difference between target and gun
                        deltaHeight = termASL - startASL;

                        // get magnitude of range from gun to term so we can calculate elevation later
                        range = Convert.ToInt32(Math.Sqrt(Math.Pow(Math.Abs(deltaX), 2) + Math.Pow(Math.Abs(deltaY), 2)));

                        // find angle from start to term in (converted from radians to) degrees
                        double angleXY = (Math.Atan2(Convert.ToDouble(deltaY), Convert.ToDouble(deltaX)) * (180 / Math.PI));
                        double bearingXY = math2comp(angleXY);
                        //if (angleXY > 360) { angleXY -= 360; }
                        //else if (angleXY < 0) { angleXY += 360; }

                        // convert target bearing from angle degrees to mils
                        int AzimuthMILS = Convert.ToInt32(bearingXY / 360 * 6400);
                        AzimuthMILS_str = AzimuthMILS.ToString();
                        while (AzimuthMILS_str.Length < 4) { AzimuthMILS_str = "0" + AzimuthMILS_str; };
                    }
                    else
                    {
                        MessageBox.Show("Invalid spotter grid or target BRA.");
                    }
                }*/
            }
            else
            {
                MessageBox.Show("Invalid gun grid.");
            }


            if (range != -1 && deltaHeight != -1)
            {
                // set range card values equal to 0 to initialize
                int elev = 0;
                int elev_d = 0;
                double timer = 0;
                double timer_d = 0;
                int requiredCharge = 0;

                // set a var to break the outer for loop
                bool canbreakloops = false;

                // iterate through all of the ranges and find the charge that gets us to the range
                for (int charge = 0; charge < M252.ranges.Length; charge++)
                {
                    for (int rangeInd = 0; rangeInd < M252.ranges[charge].Length; rangeInd++)
                    {
                        // if the range number is an actual number in the chart
                        if (M252.ranges[charge][rangeInd] == range)
                        {
                            elev = M252.elev[charge][rangeInd];
                            elev_d = M252.elev_d[charge][rangeInd];
                            timer = M252.timer[charge][rangeInd];
                            timer_d = M252.timer_d[charge][rangeInd];
                            requiredCharge = charge;

                            canbreakloops = true;
                            break;
                        }
                        else if (rangeInd + 1 <= M252.ranges[charge].Length - 1)
                        {
                            // if our range is above the current indexed range and below the next, we found our bounds
                            if (range > M252.ranges[charge][rangeInd] && range < M252.ranges[charge][rangeInd + 1])
                            {
                                // we need to know our bounds so we can use linear interpolation to get the exact value we are supposed to have
                                int range_low = M252.ranges[charge][rangeInd];
                                int range_high = M252.ranges[charge][rangeInd + 1];

                                int elev_low = M252.elev[charge][rangeInd];
                                int elev_high = M252.elev[charge][rangeInd + 1];
                                elev = Convert.ToInt32(linint(range_low, range_high, elev_low, elev_high, range));

                                int elev_d_low = M252.elev_d[charge][rangeInd];
                                int elev_d_high = M252.elev_d[charge][rangeInd + 1];
                                elev_d = Convert.ToInt32(linint(range_low, range_high, elev_d_low, elev_d_high, range));

                                double timer_low = M252.timer[charge][rangeInd];
                                double timer_high = M252.timer[charge][rangeInd + 1];
                                timer = linint(range_low, range_high, timer_low, timer_high, range);

                                double timer_d_low = M252.timer_d[charge][rangeInd];
                                double timer_d_high = M252.timer_d[charge][rangeInd + 1];
                                timer_d = linint(range_low, range_high, timer_d_low, timer_d_high, range);

                                requiredCharge = charge;

                                canbreakloops = true;
                                break;
                            }
                        }
                        // if we haven't found an exact match, and our next test will exceed the number of indices anyways,
                        // skip ahead out of the loop and start searching the next charge
                        else { break; }

                    }
                    if (canbreakloops)
                    {
                        break;
                    }
                } // if (!canbreakloops) { MessageBox.Show("broken shit"); } // somehow we looped through everything and didn't find a range that works. shits def broke

                // calculate our elevation of gun
                int Elevation = Convert.ToInt32(elev + (elev_d * (Convert.ToDouble(-deltaHeight) / 100)));
                Elevation_str = Elevation.ToString();

                // calculate our time of flight for projectiles
                double FlightTime = Math.Round(timer + (timer_d * (Convert.ToDouble(-deltaHeight) / 100)), 1);
                FlightTime_str = FlightTime.ToString();

                // and after all that, which charge were we using??
                Charge_str = requiredCharge.ToString();
            }

            clearSolution();
            string[] solution = new string[4] { Charge_str, AzimuthMILS_str, Elevation_str, FlightTime_str };
            return solution;
        }
        
        private string[] adjustArtillerySolution()
        {
            string[] currentsolution = new string[4] { solutionCharge.Text, solutionAzimuth.Text, solutionElevation.Text, solutionTime.Text };
            if (artyLeft.IsChecked == true || artyRight.IsChecked == true || artyUp.IsChecked == true || artyDown.IsChecked == true)
            {
                int latAdjust = 0;
                int vertAdjust = 0;
                if (artyLeft.IsChecked == true || artyRight.IsChecked == true)
                {
                    if (Int32.TryParse(adjustLRbox.Text, out latAdjust))
                    {
                        if (artyLeft.IsChecked == true)
                        {
                            latAdjust = -1 * latAdjust;
                        }
                    }
                    else { adjustLRbox.Text = "0"; }
                }

                if (artyUp.IsChecked == true || artyDown.IsChecked == true)
                {
                    if (Int32.TryParse(adjustUDbox.Text, out vertAdjust))
                    {
                        if (artyDown.IsChecked == true)
                        {
                            vertAdjust = -1 * vertAdjust;
                        }
                    }
                    else { adjustUDbox.Text = "0"; }
                }

                if (latAdjust == 0 && vertAdjust == 0)
                {
                    return currentsolution;
                }

                // convert gun input to 5 digit grid for each direciton (10 digit total grid read-out)
                while (gungridWE.Text.Length < 5) { gungridWE.Text += "0"; };
                while (gungridNS.Text.Length < 5) { gungridNS.Text += "0"; };

                // get our starting position turned into numbers
                if (Int32.TryParse(gungridWE.Text, out int startX) && Int32.TryParse(gungridNS.Text, out int startY) && Int32.TryParse(gungridASL.Text, out int startASL))
                {
                    // convert target input to 5 digit grid for each direciton (10 digit total grid read-out)
                    while (spottergridWE.Text.Length < 5) { spottergridWE.Text += "0"; };
                    while (spottergridNS.Text.Length < 5) { spottergridNS.Text += "0"; };

                    // get our spotter position and target reference turned into numbers - we are not concerned with target height while calculating azimuth
                    if (Int32.TryParse(spottergridWE.Text, out int spotX) && Int32.TryParse(spottergridNS.Text, out int spotY) && Int32.TryParse(targetBearing.Text, out int tgtBearing)
                        && Int32.TryParse(targetRange.Text, out int tgtRange) && Int32.TryParse(targetgridASL.Text, out int termASL))
                    {
                        // get grid location of termination point. convert bearing to radians, then calculate target position from spotter position
                        double tgtAbsAngle = comp2math(Convert.ToDouble(tgtBearing) * 360 / 6400);

                        int termX = Convert.ToInt32(spotX + tgtRange * Math.Cos((tgtAbsAngle) / 180 * Math.PI)
                            + latAdjust * Math.Cos((tgtAbsAngle - 90) / 180 * Math.PI) + vertAdjust * Math.Cos(tgtAbsAngle / 180 * Math.PI));
                        int termY = Convert.ToInt32(spotY + tgtRange * Math.Sin((tgtAbsAngle) / 180 * Math.PI)
                            + latAdjust * Math.Sin((tgtAbsAngle - 90) / 180 * Math.PI) + vertAdjust * Math.Sin(tgtAbsAngle / 180 * Math.PI));

                        targetgridWE.Text = termX.ToString();
                        targetgridNS.Text = termY.ToString();

                        while (targetgridWE.Text.Length < 5) { targetgridWE.Text = "0" + targetgridWE.Text; };
                        while (targetgridNS.Text.Length < 5) { targetgridNS.Text = "0" + targetgridNS.Text; };

                    }
                    else
                    {
                        MessageBox.Show("Invalid spotter grid or target reference.");
                    }

                    artyLeft.IsChecked = false;
                    artyRight.IsChecked = false;
                    artyUp.IsChecked = false;
                    artyDown.IsChecked = false;
                    adjustLRbox.Text = String.Empty;
                    adjustUDbox.Text = String.Empty;

                    //clearSolution();
                    //string[] solution = new string[4] { Charge_str, AzimuthMILS_str, Elevation_str, FlightTime_str };
                    return findArtillerySolution();
                }
            }

            // make no changes if any errors occur such as missing numbers, bad grid, etc
            return currentsolution;
        }

        // set up a function we can use to get the exact value on a line between two points if we have the X value for a third
        // pro data science move
        private double linint(double X1, double X2, double Y1, double Y2, double Xneed)
        {
            double Yneed = (((Xneed - X1) * (Y2 - Y1)) / (X2 - X1)) + Y1;
            return Yneed;
        }

        // convert mathematical angle to compass bearing
        private double math2comp(double mathDeg)
        {
            double compassBearing = (mathDeg * -1) + 90;
            if (compassBearing > 360) { compassBearing -= 360; }
            else if (compassBearing < 0) { compassBearing += 360; }
            return compassBearing;
        }

        // convert compass bearing to mathematical angle
        private double comp2math(double compassBearing)
        {
            double mathDeg = (compassBearing * -1) + 90;
            if (mathDeg > 360) { mathDeg -= 360; }
            else if (mathDeg < 0) { mathDeg += 360; }
            return mathDeg;
        }

        private void clearbutton_Click(object sender, RoutedEventArgs e)
        {
            guntypebox.SelectedIndex = -1;
            gungridWE.Text = String.Empty;
            gungridNS.Text = String.Empty;
            gungridASL.Text = String.Empty;

            spottergridWE.Text = String.Empty;
            spottergridNS.Text = String.Empty;

            targetBearing.Text = String.Empty;
            targetRange.Text = String.Empty;

            targetgridWE.Text = String.Empty;
            targetgridNS.Text = String.Empty;
            targetgridASL.Text = String.Empty;

            clearSolution();
        }

        private void clearSolution()
        {
            solutionCharge.Text = String.Empty;
            solutionAzimuth.Text = String.Empty;
            solutionElevation.Text = String.Empty;
            solutionTime.Text = String.Empty;
        }
    }
}
