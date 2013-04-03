# EmbeddedMail

## Overview

Integration testing against e-mail almost always sucks. EmbeddedMail doesn't remove all of that pain, but it certainly helps it suck less. It's just a simple class library with **0 dependencies**.

### Show me the code

Here's some simple example code:

	var server = new EmbeddedSmtpServer(8181);
	server.Start();
	
	var message = new MailMessage("x@domain.com", "y@domain.com", "Hello there", "O hai");
	using(var client = new SmtpClient("localhost", 8181))
	{
		client.Send(message);
	}
	
	server.Stop();
	
	var received = server.ReceivedMessages().First();
	
	received.Body.ShouldEqual(message.Body);
	received.Subject.ShouldEqual(message.Subject);

### How it works

The idea is that you collapse your app-domain for integration-testing purposes by embedding an SMTP server inside of your tests. You then run some code that would shoot out emails, stop the smtp server, and make assertions on the messages that were received (e.g., "The body should be...")

### NuGet

> Install-Package EmbeddedMail

### Shout outs

I took a look at some of the existing solutions out there and found two:

* netDumpster
* nDumpster

Some of the message parsing "borrows" some code found in netDumpster (thanks Carlos and Eric for your contributions -- your work is cited). 

Also, I borrowed some socket/logging code from Jason Staten and his excellent Fleck library.
