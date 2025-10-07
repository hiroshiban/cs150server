// SPDX-License-Identifier: BSD-2-Clause
/**
 * @file    cs150server.cs
 * @brief   A tiny stdio command server to operate Konica Minolta CS‑150/CS‑160
 *          from MATLAB and Python (bridging MATLAB/Python and LC‑MISDK through
 *          an external process).
 *
 * @details
 *  This program inserts a minimal out‑of‑process server between MATLAB (`cs150.m`)
 *  and Konica Minolta’s LC‑MISDK to avoid in‑process DLL/.NET loading issues and
 *  improve robustness. It reads one text command per line from stdin and writes
 *  CSV responses to stdout.
 *
 *  [Licensing & Redistribution]
 *   - This repository ships only original code. No vendor SDK/DLL/manuals are included.
 *     Obtain LC‑MISDK legitimately and follow the vendor’s license terms.
 *   - Do NOT commit or redistribute vendor DLLs with this source tree.
 *
 *  [Supported instruments]
 *   - Default backend targets CS‑150/CS‑160 (via LC‑MISDK).
 *   - CS‑200 requires a different driver/DLL and is out of scope here (can be added as
 *     a separate backend/target).
 *
 *  [Command protocol (MATLAB ⇄ server)]
 *   - CONNECT            -> SUCCESS,Connected to CS-150
 *   - INTEG <sec>|AUTO   -> SUCCESS,Integration time set
 *   - MEASURE            -> SUCCESS,<Lv(cd/m^2)>,<x>,<y>
 *   - DISCONNECT         -> SUCCESS,Disconnected (or silent)
 *   - EXIT               -> (process terminates)
 *   * Non-'SUCCESS' responses indicate errors.
 *   * Numbers are emitted with dot decimal (InvariantCulture semantics).
 *
 *  [Build requirements (example)]
 *   - Windows 10/11
 *   - MSVC / Visual Studio 2019+ (C++17 or later recommended)
 *   - If using C++/CLI to call .NET APIs: enable /clr and reference .NET Framework 4.6.1+
 *   - Place vendor DLLs next to the executable at runtime only; keep them out of the repo
 *   - Suggested flags: /std:c++17 /W4 /permissive- /utf-8 /EHsc /DUNICODE /D_UNICODE
 *
 *  [Design notes]
 *   - Single-threaded line-processing loop on stdin; emits machine-readable stdout only.
 *   - Exit code 0 on success; 1 on fatal initialization failures.
 *   - Operational logs (if any) should go to stderr to keep stdout parseable.
 *
 *  @author   (Your Name / Affiliation)
 *  @version  (e.g., 1.0.0)
 *  @date     (YYYY-MM-DD)
 *  @license  MIT for the original code. Vendor SDKs/drivers remain under their own licenses.
 *  @see      README.md (setup, caveats, extensions)
 *  @note     Unofficial project; provided “as is,” without warranty of any kind.
 *
 *
 * Created    : "2025-09-30 15:02:28 ban"
 * Last Update: "2025-10-07 18:12:48 ban"
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Konicaminolta;

namespace cs150server
{
    class Program
    {
        // holding the sdk and connection status as class members
        private static LightColorMISDK sdk;
        private static bool isConnected = false;

        static void Main(string[] args)
        {
            // Configure console for UTF-8 & auto-flush to ensure MATLAB/Python can read lines promptly
            Console.OutputEncoding = Encoding.UTF8;
            var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(stdout);
            var stderr = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(stderr);

            try
            {
                sdk = LightColorMISDK.GetInstance();
            }
            catch (Exception ex)
            {
                // terminating the execute when initialization is failed.
                Console.Error.WriteLine("FATAL: SDK GetInstance failed. " + ex.Message);
                return;
            }

            // infinite loop to wait for the command input(s) from MATLAB
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                // separate the input string by space
                string[] parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue; // ignore an empty line

                string command = parts[0].ToUpper(); // commande (convert to upper case)
                string argument = parts.Length > 1 ? parts[1] : null; // parameter (if any)

                // select the operation based on the input command.
                switch (command)
                {
                    case "CONNECT":
                        HandleConnect();
                        break;

                    case "MEASURE":
                        HandleMeasure();
                        break;

                    case "INTEG":
                        HandleSetIntegrationTime(argument);
                        break;

                    case "BACKLIGHTON":
                        HandleBackLightON();
                        break;

                    case "BACKLIGHTOFF":
                        HandleBackLightOFF();
                        break;

                    case "DISCONNECT":
                        HandleDisconnect();
                        break;

                    case "EXIT":
                        HandleDisconnect();
                        return;

                    default:
                        Console.WriteLine("ERROR,Unknown command");
                        break;
                }
            }
        }

        private static void HandleConnect()
        {
            if (isConnected)
            {
                Console.WriteLine("SUCCESS,Already connected");
                return;
            }
            try
            {
                ReturnMessage ret = sdk.Connect();
                if (ret.errorCode == ErrorDefine.KmSuccess)
                {
                    isConnected = true;
                    Console.WriteLine("SUCCESS,Connected to CS-150");
                }
                else
                {
                    Console.WriteLine("ERROR," + ret.errorCode.ToString());
                }
            }
            catch (Exception ex)
            {
                // capture the error related to the SimpleInjector
                Console.WriteLine("ERROR," + ex.Message);
            }
        }

        private static void HandleMeasure()
        {
            if (!isConnected)
            {
                Console.WriteLine("ERROR,Not connected");
                return;
            }

            // simple xyY measurement
            var ret = sdk.Measure();
            if (ret.errorCode != ErrorDefine.KmSuccess)
            {
                Console.WriteLine("ERROR,Measure command failed");
                return;
            }

            MeasStatus state;
            do
            {
                System.Threading.Thread.Sleep(100);
                ret = sdk.PollingMeasurement(out state);
                if (ret.errorCode != ErrorDefine.KmSuccess)
                {
                    Console.WriteLine("ERROR,Polling failed");
                    return;
                }
            } while (state == MeasStatus.Measuring);

            Lvxy lvxyValue = new Lvxy(LuminanceUnit.cdm2);
            ret = sdk.ReadLatestData(lvxyValue);
            if (ret.errorCode != ErrorDefine.KmSuccess)
            {
                Console.WriteLine("ERROR,ReadLatestData failed");
                return;
            }

            // output the measurement result as a string with "," as a delimiter
            Console.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "SUCCESS,{0},{1},{2}", lvxyValue.Lv, lvxyValue.x, lvxyValue.y));
        }

        private static void HandleSetIntegrationTime(string argument)
        {
            if (!isConnected)
            {
                Console.WriteLine("ERROR,Not connected");
                return;
            }

            if (string.IsNullOrEmpty(argument))
            {
                Console.WriteLine("ERROR,Integration time value is missing. Use 'INTEG AUTO' or 'INTEG <seconds>'.");
                return;
            }

            // MeasurementTime object
            MeasurementTime inputTime = new MeasurementTime();

            // whether the parameter is "AUTO" or not.
            if (argument.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
            {
                inputTime.MeasTimeMode = MeasTimeMode.Auto;
            }

            else
            {
                // parse with invariant culture (dot decimal)
                if (double.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out double timeInSeconds))
                {
                    inputTime.MeasTimeMode = MeasTimeMode.Manual;
                    inputTime.ManualMeasurementTime = timeInSeconds;
                }
                else
                {
                    // failed to convert the input parameter to an integer
                    Console.WriteLine("ERROR,Invalid time value. It must be 'AUTO' or a number.");
                    return;
                }
            }

            // set integration time
            var ret = sdk.SetMeasurementTime(inputTime);
            if (ret.errorCode == ErrorDefine.KmSuccess)
            {
                Console.WriteLine("SUCCESS,Integration time set");
            }
            else
            {
                Console.WriteLine("ERROR,Failed to set integration time with code: " + ret.errorCode.ToString());
            }
        }

        private static void HandleBackLightON()
        {
            if (!isConnected)
            {
                Console.WriteLine("ERROR,Not connected");
                return;
            }

            // Set the backlight on
            BackLightMode mode = BackLightMode.On;
            var ret = sdk.SetBackLightOnOff(mode);
            if (ret.errorCode == ErrorDefine.KmSuccess)
            {
                Console.WriteLine("SUCCESS, Backlight ON");
            }
            else
            {
                Console.WriteLine("ERROR," + ret.errorCode.ToString());
            }
        }

        private static void HandleBackLightOFF()
        {
            if (!isConnected)
            {
                Console.WriteLine("ERROR,Not connected");
                return;
            }

            // Set the backlight off
            BackLightMode mode = BackLightMode.Off;
            var ret = sdk.SetBackLightOnOff(mode);
            if (ret.errorCode == ErrorDefine.KmSuccess)
            {
                Console.WriteLine("SUCCESS, Backlight OFF");
            }
            else
            {
                Console.WriteLine("ERROR," + ret.errorCode.ToString());
            }
        }

        private static void HandleDisconnect()
        {
            if (isConnected)
            {
                sdk.DisConnect(0); // disconnection
                isConnected = false;
            }
        }
    }
}
