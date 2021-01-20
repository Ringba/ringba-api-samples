# ringba-api-samples

## Call Logs

Lets hypothetically assume we need to extract the following data from each call:
`Caller Id, Inbound Number, Target Number, Call Length, Connected Call Length, State (region), Date Time, and Call Status (live, completed)`

To get the call logs you will need to send a request to Ringba's API Call Log endpoint. This will return a JSON object with many fields with information tailored to feed our report with all the data it needs to render on the application. For the purpose of extracting the call data we are interested in, we will be looking at the array in this JSON under `callLog.data[ ]`, this array contains all the call info. Below is a semi-collapsed view of a sample JSON response to exemplify what the program gets back from the API.

![image](https://user-images.githubusercontent.com/50965686/70721622-aed41300-1cd4-11ea-8df1-2192498d45b6.png)

This `data` object contains an array with all the calls under which each element is a call record and it divides into three main data grouping.
  * `Columns`: Contains all main information regarding a call;
  * `Events`: Events at which the call has gone through. Such as when it was answered by the target, when it was hangup. These have its own columns information with data related to the given event 
  * `Tags`: Lists all the tag information this call has been attached to, such as the caller's details (country, state, number) as well as the campaign's, target's and publisher's data (name, id).

This is also how it is divided on Ringba's UI report so you can easily see where each data comes from.

That is basically how the call log response is structured. That said, the application we provided was designed to take a user (email for a registered Ringba account), password and the accountID information to consume Ringba's API and return the call log for the given time range. It is set as default to query it for today, but can be easily managed to search for different time range on `service.GetCallLogsAsync` consumption.

The program goes over the response list and uses a conversion helper to turn objects into dictionary at `JsonElementHelper.cs`, which is applied to all three data grouping mentioned previously and makes it easy for the application to collect key data its need on each call. That done, it uses the dictionary lookup for grabbing the information it needs, that is how it queries columns to collect inboundPhoneNumber, the callLengthInSeconds, the events to search for CompletedCall and EndCallSource to know whether the call is still live, and the tags to pull the caller's state.

This sample program should give a basic understanding of how Ringba's `CallLogs` works and should be enough for moving forward. If one wants different fields in the final output the code can be easily modified. 
