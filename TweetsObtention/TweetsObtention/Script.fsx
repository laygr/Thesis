#I @"../packages/FSharp.Data.Toolbox.Twitter.0.19/lib/net40"
#I @"/Users/laygr/Projects/TweetsObtention/packages/FSharp.Data.2.2.3/lib/net40"

#r "FSharp.Data.Toolbox.Twitter.dll"
#r "FSharp.Data.dll"

open FSharp.Data.Toolbox.Twitter

let key = "mVDRszMycAMgKFbreF6U4Ka1O"
let secret = "dBp7wHVFMuBLn92z0h82LRsM14O921FJBkXv75cOBElSchlLAM"

let twitter = Twitter.AuthenticateAppOnly(key, secret)

(*
let connector = Twitter.Authenticate(key, secret) 

// Run this part after you obtain PIN
let twitter = connector.Connect("7808652")
*)

let fsharpTweets = twitter.Search.Tweets("#fsharp", count=100)

for status in fsharpTweets.Statuses do
  printfn "@%s: %s" status.User.ScreenName status.Text