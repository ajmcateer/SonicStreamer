# SonicStreamer
Listen to your own music with SonicStreamer on Windows 10.   
[Subsonic](http://www.subsonic.org/) is a media streamer where you can get access to music which is stored on a server. Instead using the web browser you can use SonicStreamer as a Client App. Just connect the App to a Subsonic server and you will be able to stream all the music from this server.

Please keep in mind that you probably need a Premium License to use the online functionalities. For more information about Subsonic and how to set up a server please visit the [Subsonic homepage](http://www.subsonic.org/pages/premium.jsp).

## Architecture
The whole app uses the [Model–view–view-model (MVVM) pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) as usual for Universal Windows Platform (UWP) Apps. Beside the three levels Model, ViewModel, View there is a fourth level called "back end". This level takes over the whole communication to external server like Subsonic and its derivates or other services like last.fm or MusicBrainz.

## 3rd party libraries

The following 3rd party libraries are used:

### MusicBrainz

C# library which implements the MusicBrainz API.

- Author: [avatar29A](https://github.com/avatar29A)
- Source: [GitHub](https://github.com/avatar29A)
- License: [The MIT License (MIT)](https://github.com/avatar29A/MusicBrainz/blob/master/LICENSE.txt)

### Json.NET Schema

Json.NET Schema is a powerful, complete and easy to use JSON Schema framework for .NET

- Author: [Newtonsoft](http://www.newtonsoft.com/)
- Source: [GitHub](https://github.com/JamesNK/Newtonsoft.Json.Schema)
- License: [GNU Affero General Public License Version 3](http://www.gnu.org/licenses/agpl-3.0.html)