# AzureMessageProcessing
This repository is a load test for Azure Storage and Azure Functions.

It has generators for message objects that are put into an on-ramp queue and their contents are put into a blob storage.
Azure Functions take those messages, run steps on them and put them back into the queue. Eventually, when all steps are ran on a message, a copy of it is added to table storage.

We tested the code against Azure, running 102 000 messages in 1.5 hours.
There were no slowdowns, execution got faster towards the end and all messages were processed.
All messages got processed in seconds. Eventual cost was 0.35€, half Functions, half Storage. Functions ran for 450 000 times and with 600 000 GB/s.
Since we barely pushed out of free tier, the costs should increase rapidly but they are still really low.
