# ConnectionMonitor

Inspired by [https://github.com/camerondaly/connection-monitor](@CameronDaly/connection-monitor),
I was dealing with similar issues and wanted to have a solution in the language I was most comfortable with.
A local ISP monopoly was claiming that my connection was fine, but I was experiencing frequent outages.

I was able to leave this running overnight and capture the outages as they happened.
The technician that came out was able to see the logs and confirm that there was an issue with the equipment they had installed.

## Usage

Run the application.  It will log the status of your connection to the console and the log file.

To exit the console app, use Ctrl + C.

## Configuration

By default, the application will create a socket connection to Google's Public DNS Server (8.8.8.8) using port 53.

You can change the IP address and port by editing the `config.json` file.

You can also change the sleep interval in milliseconds and the connection timeout in seconds by editing the `config.json` file.
