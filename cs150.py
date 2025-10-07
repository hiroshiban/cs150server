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


if __name__ == '__main__':
    # === Demonstration of how to use this script ===
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

    # When exiting the 'with' block, the photometer.close() is automatically called
