# AvaMSN
An Avalonia MSNP15 client. It's compatible with Linux, Windows and MacOS.

## Server
The default server is [CrossTalk](https://crosstalk.hiden.cc), which is currently in private alpha. That can be changed in the options window, however, to any server supporting version 15 of the protocol.

## MacOS notes
For the notifications feature to work on macOS the client has to be signed. As the $99 yearly developer ID  price is too much for me (especially in my currency), I used a self-signed certificate. So in order to get notifications working you need to install and trust this certificate, which will come with the app bundle as a .cer file when downloaded.

## To do
- [x] Come up with a better way to do the "remember your password" feature
- [x] Finish message formatting
- [ ] Offline messages
- [x] Tray icon
- [x] Native notifications

![avamsn](https://github.com/campos02/AvaMSN/assets/45215327/063441f5-008e-4885-a227-a16eb21e7c74)
