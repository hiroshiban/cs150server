# **README on cs150server**

<div align="right">
Created    : "2025-10-07 11:33:12 ban"<br>
Last Update: "2025-10-07 17:13:14 ban"
</div>

<br>
<div align="center">
<img src="https://img.shields.io/badge/LANGUAGE-MATLAB%20&%20Python-brightgreen" />
<img src="https://img.shields.io/badge/EDITED%20BY-EmEditor%20&%20VS%20Code-blue" />
<img src="https://img.shields.io/badge/LICENSE-BSD-red" /><br>
<img src="https://img.shields.io/badge/KEYWORDS-Vision%20Science,%20Photometer,%20Colorimeter,%20Gammma%20Correction,%20Luminance,%20Color,%20Chromaticity,%20Psychtoolbox,%20PsychoPy,%20Konica--Minolta-blue?style=social&logo=webauthn" /><br>
<img src="https://img.shields.io/badge/CONTACT-lightgrey" /> <img src="doc/images/ban_hiroshi_address.png" />
</div>
<br>

# <a name = "Menu"> **cs150server: MATLAB/Python-CS150 Bridge -- Control Konica-Minolta CS-150/160 from MATLAB/Python seamlessly via a tiny C# command server.** </a>

![cs150server](doc/images/cs150server.jpg)

## Introduction -- What is cs150server? Why is it required?
This repository provides tiny but useful (hopefully) tools to control Konica-Minolta CS-150/160 colorimeters from MATLAB/Python. After the update of the colorimeter from CS-100A to CS-150, Konica-Minolta discontinued to provide low-level libraries to communicate the colorimeter through a simple serial communication protocol. Instead, the manufacture now provides a bit higher-level C# libraries only. This update may be useful in some specific situations, while it makes the communications from the external (non-C#) programs, such as MATLAB and Python, difficult. Therefore, I developed simple tools to control the CS-150/160 colorimeter from MATLAB/Python via a tiny C# command server.  
  
**NOTE: if you copy cs150.m and cs150server directory to ~/subfunctions/colorimeter directory of my inhouse software package, [Mcalibrator2](https://github.com/hiroshiban/Mcalibrator2), you can use cs-150 with Mcalibrato2 on MATLAB.**  


**The repository contains *only* original source codes**: a MATLAB client (`cs150.m`), a Python client (`cs150.py`), and a lightweight C# command server (`cs150server.exe`) that you build locally.  
  
> **No Konica-Minolta DLLs or SDK files are distributed here, following the license and EURA provided by the manufacture.** You must obtain the official **LC‑MISDK** from Konica-Minolta website, and follow their license terms.  
  
Konica-Minolta’s **LC‑MISDK** is a .NET SDK for **CS-150/160** (and **LS-150**) luminance/color meters. Direct loading of the C# DLLs, for instance, by calling the AddAssenbly() function inside MATLAB would stack for its complicated dependencies on the external libraries. To overcome this difficulty, this project inserts a **minimal C# server** between MATLAB/Python and the SDK; MATLAB/Python sends text commands over stdin/stdout, and the server returns measured values as CSV.  
  
LC‑MISDK officially lists **CS-150/160 (and LS-150/160)** as compatible instruments and supports building both **32‑bit and 64‑bit** applications.  

> **Note on CS‑200:** The **CS‑200** uses a separate USB driver package with its own DLL reference and communication spec. That driver is **32‑bit only** (apps must be 32‑bit even on 64‑bit OS). If you need CS‑200, please implement a dedicated backend (separate branch/target).  

## Internal communication at a glance

[MATLAB(cs150.m)/Python(cs150.py)] ⇄ [stdin/stdout] ⇄ [C# server:cs150server.exe] ⇄ [LC-MISDK] ⇄ [USB driver] ⇄ [CS-150/160]  
  
- MATLAB sends textual commands: `CONNECT`, `INTEG 0.5`, `MEASURE`, …  
- Server calls LC‑MISDK and returns `SUCCESS,<Lv(cd/m^2)>,<x>,<y>` or `ERROR,<reason>`.  
- Tested on Windows 10/11.  

## Licensing & compliance (read me first)

- **No redistribution of vendor SDK/DLLs.** This repo ships *only* original code. Obtain **LC‑MISDK** from Konica Minolta's official download page and follow the EULA included there. 
- **CS‑200** materials (USB driver, DLL Reference, comms spec) are provided on a separate official page for owners; follow those license terms as well.  
- Add a `.gitignore` to prevent accidental commits of binaries:  
  *.dll
  *.exe
  cs150server/bin/
  cs150server/obj/
  packages/

## System requirements

- **OS:** Windows 10/11  
- **.NET:** .NET Framework **4.6.1+** (4.7–4.8 recommended)  
- **IDE:** Visual Studio 2022 (Community is fine)  
- **MATLAB:** R2022a+ (tested, for stable `.NET` interop and `System.Diagnostics.Process`)  
- **SDK:** **LC‑MISDK** for CS‑150/CS‑160 (download and extract ZIP; follow the Reference Manual). LC‑MISDK supports building **32‑bit and 64‑bit** apps and lists **CS‑150/CS‑160 / LS‑150/LS‑160** as compatible. :contentReference[oaicite:6]{index=6}

> **If you need CS‑200**: Use the official **CS‑200 USB Driver** package. It’s a **32‑bit driver**; your app must be **x86** even on 64‑bit Windows. :contentReference[oaicite:7]{index=7}

## Directory layout

this_repo/  
├─ cs150.m (a MATLAB class which communicate with cs150server)  
├─ cs150.py (a Python class which communicate with cs150server)  
├─ cs150server_csharp/ # C# .NET Framework Console App (this repo)  
│ ├─ cs150server.cs (source code)  
│ ├─ cs150server.csproj  
│ └─ (other .cs files)  
└─ docs/  
   └─ images/ # screen shots etc  

## Building the C# server (cs150server)

Since this repository can not contains any DLLs distributed by Konica-Minolta and the built exexutable, I will leave step-by-step instructions on how to build the cs150.exe as below.  

#### **Step 0:** Installing the cs150 device driver to your Windows PC.  

1. Open the "Device Manager" by right-clicking the Windows START icon. Then, open the "Ports (COM & LPT)" tab.  

![driver_01](doc/images/driver/00_before_installing_device_driver.png)

2. Select the "USB Serial Device (COM ***)" item, and right click it. Then, select the "Update driver."  

![driver_02](doc/images/driver/01_updating_device_driver.png)

3. Select the "Browse my computer for drivers."  

![driver_03](doc/images/driver/02_selecting_device_driver.png)

4. Set the cs150 driver in LC-MISDK distributed by Konica-Minolta, and select "Next."  

![driver_04](doc/images/driver/03_selecting_KonicaMinolta_driver.png)

5. Press the "Install" button.  

![driver_05](doc/images/driver/04_installing_device_driver.png)

6. Once done, the driver will be updated. Please check the name "KONICA MINOLTA, INC." is found in the driver provider section.  

![driver_06](doc/images/driver/05_device_driver_updated.png)
  
Now, we are ready for building cs150server.exe.  

#### **Step 1:** Building cs150server

1. Create a .NET Framework Console App in Visual Studio (name it as 'cs150server'). Specifically, select "Create a new project" and then select "Console App (.NET Framework)." Please don't select just a simple "Console App" without .NET Framework. Finally, please set the project name, locations etc in the following input window.  

![server_01](doc/images/server/01_launching_VisualStudio.png)
![server_02](doc/images/server/02_selecting_console_app_dot_net.png)
![server_03](doc/images/server/03_making_project.png)

Then, please copy "nupkg" and "packages" directories to the generated cs150server directory.

The generated project directory.  

![server_09](doc/images/server/09_default_explorer_contents.png)

After copying the "nupkg" and "packages" directories.  
![server_10](doc/images/server/10_adding_required_packages.png)

2. The generated windows will look as below.  

![server_04](doc/images/server/04_initial_window.png)

3. Then, install the required libraries using the "nupkg" package system (= adding references to the LC-MISDK assemblies distributed by Konica-Minolta). Firstly, please right-click the "cs150server" shown on the upper right panel. Then, in the popup window, please select the "Manage NuGet Packages..."  

![server_05](doc/images/server/05_nuget_package.png)

4. Then, install the LC-MISDK package. After the procedure 3. above, the "NuGet Packages" managing window will be shown as below.  

![server_06](doc/images/server/06_setup_nuget_package.png)

Please select the "gear" icon on the upper right corner of the main window. Then, the "Option" window will be shown as below. In this window, please press "+" button and add the source by selecting the "nuget" directoy just copied to the project in the 1. procedure above.  

![server_07](doc/images/server/07_adding_source.png)
![server_11](doc/images/server/11_selecting_nupkg_location.png)

Once added, the nuget path will be shown in the "Option " window as below.  

![server_08](doc/images/server/08_package_source_added.png)
![server_12](doc/images/server/12_package_selected.png)

If you confirm the directory is properly added as a source, please press the "OK" button and close the window.  

Next, in the "NuGet Packages" managing window, select the "package source" to install LC-MISDK. You can find the "LC-LISDK" name on the left panel. Then, please press "Install" button on the right panel. Please select "OK" and "I Accept" in the following panels.  

![server_13](doc/images/server/13_selecting_package_source.png)
![server_14](doc/images/server/14_installing_LC_MISDK.png)
![server_15](doc/images/server/15_accepting_to_install_package.png)

Once the dependent packages are installed, all the references will be added in the right project panel.  

![server_16](doc/images/server/16_package_installed.png)

The contents of the project directory (cs150server) after this step will look as below.

![server_17](doc/images/server/17_directory_contents.png)

5. Next, let's write the server source code (but actually what you need to do is just to copy...)  

Please select the Program.cs file from the right panel.  

![server_18](doc/images/server/18_before_updating_code.png)

Then, please overwrite Program.cs by copying and pasting the contents of cs150server.cpp in this repository.  

![server_19](doc/images/server/19_after_updating_code.png)

6. Finally, you are ready to build the "cs150server.exe."  

Please select the "build" in the manu bar (see the figure above). Then, from the sub menu, please select "Build Solution."  

![server_20](doc/images/server/20_building_project.png)

If no error is found, the executable will be build in ~/bin/(Release|Debug)/ directory. The dependent libraries will be also copied to the same directory.  

![server_21](doc/images/server/21_done.png)

The contens of the ~/bin/Release directory after successfully build the source.

![server_22](doc/images/server/22_exe_built.png)

7. You can now communicate with CS-150 via MATLAB and Python using the built server program.  

To do this, for instance, with MATLAB, please rename "(Release|Debug)" directory as "cs150server" and place this with cs150.m (or cs150.py in Python case) as below.  

![server_23](doc/images/server/23_organized_directory.png)

If you need to customize the subroutine, please refer the LC-MISKD documents and modify the source of CS150.m or CS150.py.  

![server_24](doc/images/server/24_write_cs150_m.png)

### cs150server.cs (stdio server)

```csharp
// SPDX-License-Identifier: BSD
/**
 * @file    CS150server.cpp
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
 * Last Update: "2025-10-07 14:26:46 ban"
 */

using System;
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
            // when this execute is launched, sdk is initialized following the standard procedure and contexts.
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
            Console.WriteLine($"SUCCESS,{lvxyValue.Lv},{lvxyValue.x},{lvxyValue.y}");
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
                // if not "AUTO", convert it to an integer
                if (double.TryParse(argument, out double timeInSeconds))
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
```

### MATLAB client (cs150.m)

Place cs150.m next to a cs150server/ directory that contains your built cs150server.exe (plus any SDK-managed dependencies).  

your_project/
├─ cs150.m
└─ cs150server/
   └─ cs150server.exe # you have to built this locally
   └─ all dependent DLLs

### cs150.m (a MATLAB class which calls cs150server as a subprocess)

```MATLAB
classdef cs150 < handle
  % A class to manipulate Konica Minolta CS-150
  % by launching and controlling a custom-made C# server executable.
  % This class and the following methods will work only on the "Windows" OS.
  %
  % [usage]
  % % launching server and connect to the colorimeter
  % photometer = cs150();
  % photometer.connect();
  %
  % % setting the integration time
  % photometer.set_integration_time(0.5);
  %
  % % run measurement
  % [Y, x, y] = photometer.measure();
  % fprintf('Measurement with 0.5s integration:\n');
  % fprintf('  Luminance: %.4f cd/m^2, xy: (%.4f, %.4f)\n\n', Y, x, y);
  %
  % % close the server
  % clear photometer;
  %
  %
  % Created    : "2025-09-25 12:40:30 ban"
  % Last Update: "2025-10-06 14:38:44 ban"

  properties (SetAccess = private)
    port_name='COM1';  % a dummy variable for consistency with the other functions.
    rscom=[];          % a dummy variable for consistency with the other functions.
    process=[];      % Handle for the external C# process
    inputStream=[];  % Stream to send commands to the C# server
    outputStream=[]; % Stream to receive results from the C# server
    init_flg=false;
  end

  methods
    % constructor
    function obj=cs150(port_name)
      disp('Starting CS-150 measurement server...');

      % setting a path to cs150server.exe
      serverDir=fileparts(mfilename('fullpath'));
      serverPath=fullfile(serverDir,'cs150server','cs150server.exe');

      if ~exist(serverPath,'file')
        error('cs150server.exe not found at the expected location: %s', serverPath);
      end

      try
        % launch the external exe file by using .NET Process class.
        psInfo=System.Diagnostics.ProcessStartInfo(serverPath);
        psInfo.UseShellExecute=false;
        psInfo.RedirectStandardInput=true;  % redirecting the standard input
        psInfo.RedirectStandardOutput=true; % redirecting the standard output
        psInfo.CreateNoWindow=true;         % madking the console window invisible

        obj.process=System.Diagnostics.Process.Start(psInfo);

        % get streams for sending/receiving commands
        obj.inputStream=obj.process.StandardInput;
        obj.outputStream=obj.process.StandardOutput;

        % wait until the server is launched.
        pause(1.0);
        disp('Measurement server started.');

        if nargin==1 && ~isempty(port_name)
          obj.port_name=port_name;
        end

      catch ME
        error('Failed to start Cs150Server.exe. %s', ME.message);
      end
    end

    % destructor
    function delete(obj)
      if ~isempty(obj.process) && ~obj.process.HasExited
        disp('Shutting down measurement server...');
        % terminating command
        obj.inputStream.WriteLine('EXIT');
        % waiting for the termination of the prcesess
        obj.process.WaitForExit(3000); % for 3 sec
        obj.process.Close();
        disp('Server shut down.');

        % Here, we should unload the .NET assemby...However, accourding to MathWorks support site...
        % Rebooting is the only solution to this problem for the current Matlab Version (8.1.0.604, R2013a):
        % "The ability to unload an assembly is not available in MATLAB at this point of time. This may be
        % addressed in one of the future releases. Currently, to work around this issue, restart MATLAB."
        %
        % Therefore, we don't do anything
      end
    end

    % connect
    function obj=gen_port(obj,port_name)
      if nargin>1 && ~isempty(port_name), obj.port_name=port_name; end
      if obj.init_flg, disp('Already connected.'); return; end

      % sending 'CONNECT' command
      obj.inputStream.WriteLine('CONNECT');
      % receiving the reaction from the server
      response=char(obj.outputStream.ReadLine());

      parts=strsplit(response,',');
      if strcmp(parts{1},'SUCCESS')
        obj.init_flg=true;
        fprintf('Successfully connected: %s\n', parts{2});
      else
        error('Connection failed: %s', response);
      end
    end

    % reset a serial port connection
    function obj=reset_port(obj)
      obj.rscom=[];
      obj.init_flg=0;
      obj.disconnect();
    end

    % initialize CS-150
    function [obj,check,integtime]=initialize(obj,integtime)
      if nargin<2 || isempty(integtime), integtime=0.4; end

      % Sets the integration time of the CS-150.
      %
      % arugments:
      %   time: Can be a number (in seconds) for manual mode,
      %         or the string 'auto' for automatic mode.

      if ~obj.init_flg
        error('Not connected. Call connect() first.');
      end

      % making command string
      if isnumeric(integtime) && integtime > 0
        % command example: "INTEG 2"
        command = sprintf('INTEG %f',integtime);
      elseif ischar(integtime) && strcmpi(integtime,'auto')
        command = 'INTEG AUTO';
      else
        error('Invalid input. Time must be a positive number or the string ''auto''.');
      end

      % sending the generated command
      obj.inputStream.WriteLine(command);
      % receiving the reaction from the server
      response=char(obj.outputStream.ReadLine());

      % check the response
      if ~startsWith(response,'SUCCESS')
        check = 1;
        error('Failed to set integration time: %s', response);
      else
        check = 0;
        disp('Integration time set successfully.');
      end
    end

    % measure
    function [qq,Y,x,y,obj]=measure(obj,integtime)
      if nargin<2 || isempty(integtime), integtime=0.4; end

      if ~obj.init_flg
        error('Not connected. Call connect() first.');
      end

      % sending the 'MEASURE' command
      obj.inputStream.WriteLine('MEASURE');
      % receiving the reaction from the server
      response=char(obj.outputStream.ReadLine());

      parts=strsplit(response, ',');
      if strcmp(parts{1},'SUCCESS')
        Y=str2double(parts{2});
        x=str2double(parts{3});
        y=str2double(parts{4});
        qq=0;
      else
        Y=NaN; x=NaN; y=NaN;
        error('Measurement failed: %s', response);
        qq=1;
      end
    end

    % backlight
    function backlight(obj,mode)
      if nargin<2 || isempty(mode), mode=1; end

      if ~obj.init_flg
        error('Not connected. Call connect() first.');
      end

      % sending the 'MEASURE' command
      if mode==1
        obj.inputStream.WriteLine('BACKLIGHTON');
      else % if mode==0
        obj.inputStream.WriteLine('BACKLIGHTOFF');
      end

      % receiving the reaction from the server
      response=char(obj.outputStream.ReadLine());
      % check the response
      if ~startsWith(response,'SUCCESS')
        error('Failed to set backlight: %s', response);
      else
        disp('backlight set successfully.');
      end
    end

    % disconnect
    function disconnect(obj)
      if obj.init_flg
        obj.inputStream.WriteLine('DISCONNECT');
        obj.init_flg = false;
        obj.rscom=[];
        disp('Disconnected.');
      end
    end

  end % methods

end % classdef cs150
```

## Command protocol of cs150.m

| Command              | Format            | Example response                   |
| -------------------- | ----------------- | ---------------------------------- |
| Connect              | `CONNECT`         | `SUCCESS,Connected to CS-150`      |
| Set integration time | `INTEG <seconds>` | `SUCCESS,Integration time set`     |
| Set auto integration | `INTEG AUTO`      | `SUCCESS,Integration time set`     |
| Single measurement   | `MEASURE`         | `SUCCESS,<Lv(cd/m^2)>,<x>,<y>`     |
| Disconnect           | `DISCONNECT`      | `SUCCESS,Disconnected` (or silent) |
| Quit server          | `EXIT`            | *(process exits)*                  |


Notes:  
Decimal is dot (.). The server uses InvariantCulture.  
Any non‑SUCCESS line should be treated as an error.  

## Example of cs150.m

```MATLAB
ph = cs150();          % starts the server
ph.connect();          % connects the instrument
ph.set_integration_time(0.5);   % or: ph.set_integration_time('auto')
[Y,x,y] = ph.measure();
fprintf('Lv=%.4f cd/m^2, xy=(%.4f, %.4f)\n', Y, x, y);
clear ph              % server exits automatically
```

### Python client (cs150.py)

Place cs150.py next to a cs150server/ directory that contains your built cs150server.exe (plus any SDK-managed dependencies).  

your_project/
├─ cs150.py
└─ cs150server/
   └─ cs150server.exe # you have to built this locally
   └─ all dependent DLLs

### cs150.py (a Python class which calls cs150server as a subprocess)

```Python
import subprocess
import os
import time
import sys
from typing import Tuple

class CS150:
    """
    Python client to control Konica-Minolta CS-150/200.
    It runs Cs150Server.exe made of C# in the background and operates via inter-process communication.
    """

    def __init__(self, server_dir: str = 'cs150server'):
        """
        Initialize an instance of CS150 and start the C# server process.
        Args:
            server_dir (str): Relative path to the folder where the server exe is stored, as seen from cs150.py.
        """
        print("Starting CS-150 measurement server...")

        server_path = os.path.join(os.path.dirname(__file__), server_dir, 'cs150server.exe')

        if not os.path.exists(server_path):
            raise FileNotFoundError(f"Server executable not found at: {server_path}")

        # Flag to hide the console window in Windows
        CREATE_NO_WINDOW = 0x08000000

        # Start C# server as a sub-process
        if sys.version_info >= (3, 7):
            # Python 3.7 and later: text=True
            self.process = subprocess.Popen(
                [server_path],
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                bufsize=1,
                creationflags=CREATE_NO_WINDOW
            )
        else:
            # Python 3.6 and before: universal_newlines=True, not text=True
            self.process = subprocess.Popen(
                [server_path],
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                universal_newlines=True,
                bufsize=1,
                creationflags=CREATE_NO_WINDOW
            )

        time.sleep(1.5) # Wait a few moments for the server to fully boot up.

        if self.process.poll() is not None:
            # If the server terminates immediately after startup
            error_output = self.process.stderr.read()
            raise RuntimeError(f"Failed to start server. It exited with code {self.process.returncode}. Error: {error_output}")

        print("Measurement server started successfully.")
        self.is_connected = False

    def _send_command(self, command: str) -> str:
        """Internal method: send a command to the server and read one line of response"""
        if self.process.poll() is not None:
            raise ConnectionError("Server process has terminated unexpectedly.")

        self.process.stdin.write(command + '\n')
        self.process.stdin.flush()
        response = self.process.stdout.readline().strip()
        return response

    def connect(self):
        """Building a connection to the colorimeter"""
        if self.is_connected:
            print("Already connected.")
            return

        response = self._send_command('CONNECT')
        if response.startswith('SUCCESS'):
            self.is_connected = True
            print(f"Successfully connected: {response}")
        else:
            raise ConnectionError(f"Connection failed: {response}")

    def measure(self) -> Tuple[float, float, float]:
        """Performs a measurement and returns a tuple of (Y, x, y)."""
        if not self.is_connected:
            raise ConnectionError("Not connected. Call connect() first.")

        response = self._send_command('MEASURE')
        parts = response.split(',')

        if parts[0] == 'SUCCESS':
            Y = float(parts[1])
            x = float(parts[2])
            y = float(parts[3])
            return Y, x, y
        else:
            raise RuntimeError(f"Measurement failed: {response}")

    def set_integration_time(self, time_val):
        """
        Sets the integration time.
        Args:
            time_val: number (in seconds) or string 'auto'.
        """
        if not self.is_connected:
            raise ConnectionError("Not connected. Call connect() first.")

        if isinstance(time_val, (int, float)) and time_val > 0:
            command = f"INTEG {time_val}"
        elif isinstance(time_val, str) and time_val.lower() == 'auto':
            command = "INTEG AUTO"
        else:
            raise ValueError("Invalid input. Time must be a positive number or the string 'auto'.")

        response = self._send_command(command)
        if not response.startswith('SUCCESS'):
            raise RuntimeError(f"Failed to set integration time: {response}")
        else:
            print("Integration time set successfully.")

    def close(self):
        """Safely terminates the server process."""
        print("Shutting down measurement server...")
        if self.process.poll() is None:
            try:
                self._send_command('EXIT')
                self.process.wait(timeout=5)
            except (subprocess.TimeoutExpired, BrokenPipeError):
                self.process.kill()
        print("Server shut down.")

    def __enter__(self):
        """Methods to support 'with' syntax"""
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """Automatically call close at the end of 'with' syntax"""
        self.close()
```

## Example of cs150.py

```python
from cs150 import CS150
import time

print("--- Example: Basic and Safe Measurement with the cs150.py class ---")

try:
    # Using the 'with' syntax is safe because the server will
    # automatically terminate if an error occurs
    with CS150() as photometer:
        # 1. Establishing a connection
        photometer.connect()

        # 2. Setting the integration time as 0.5 sec
        photometer.set_integration_time(0.5)

        # 3. Measurement
        Y, x, y = photometer.measure()
        print(f"\nMeasurement Result (0.5s):")
        print(f"  Luminance (Y) = {Y:.4f} cd/m^2")
        print(f"  CIE 1931 (x, y) = ({x:.4f}, {y:.4f})\n")

        # 4. Setting the integration time as "auto"
        photometer.set_integration_time('auto')

        # 5. Re-measurment
        Y_auto, x_auto, y_auto = photometer.measure()
        print(f"Measurement Result (Auto):")
        print(f"  Luminance (Y) = {Y_auto:.4f} cd/m^2")
        print(f"  CIE 1931 (x, y) = ({x_auto:.4f}, {y_auto:.4f})")

except (FileNotFoundError, ConnectionError, RuntimeError, ValueError) as e:
    print(f"\nAn error occurred: {e}")
```

## Troubleshootings

- **"Not connected" / connect errors**
Please verify USB driver and cables; follow the LC‑MISDK Reference Manual for setup steps (included in the SDK ZIP).  
- **Windows 11**
The LC‑MISDK page lists Windows 11 Pro among supported OS.  
- **Bitness mismatch**
Build x64 for CS-150/160 with LC‑MISDK. For CS‑200, the official page notes the USB driver is 32‑bit only; build x86 apps.  
- **Decimal commas**
The server always outputs dot decimals; MATLAB’s str2double will parse these regardless of the OS locale.  

## Extending the server

- **Continuous measurement**: Add MEASURE_CONT/STOP commands and stream results.
- **Other color spaces**: LC‑MISDK exposes alternate output types (e.g., u′v′, XYZ); use the appropriate data class with ReadLatestData(...). (See Reference Manual in the SDK ZIP.) 
- **CS-200 backend**: Implement a separate build target that calls the CS‑200 DLL APIs from the official driver package (32‑bit). 

## References

- LC-MISDK (official download & docs): system requirements, supported instruments (CS-150/160, LS-150/160), 32/64-bit app support. [***LC-MISDK.*** ](https://www.konicaminolta.com/instruments/download/software/light/lc-misdk/index.html)  
- CS-200 USB Driver (official page): 32‑bit driver; DLL reference & communication spec for writing your own software. [***cs-200***](https://www.konicaminolta.com/instruments/download/software/light/cs-200/index.html) 
- CS-150 product page (specs/overview): Konica Minolta Sensing Americas. [***cs-150***](https://sensing.konicaminolta.us/us/products/cs-150-luminance-and-color-meter/)  

## **License**  

<img src="https://img.shields.io/badge/LICENSE-BSD-red" /><br>

cs150server -- MATLAB/Python–CS150 Bridge -- Control Konica-Minolta CS-150/160 from MATLAB/Python seamlessly via a tiny C# command server.  

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:  

    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in
      the documentation and/or other materials provided with the distribution

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  

The views and conclusions contained in the software and documentation are those of the authors and should not be interpreted as representing official policies, either expressed or implied, of the FreeBSD Project.  
  
**Konica Minolta SDKs/drivers/manuals are not included and are governed by their respective licenses/EULAs.**  
