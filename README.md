## What is BirdBrain ?

It is a Membership (ExtendedMembership too!) and Role Provider for ASP.NET MVC.

## Why do I care ?

Because having to use SQL for something that does not need it sucks.

## But SQL is the best!

I can't help you...

## What's missing ?

Multiple application support, mostly because I think the use case for a shared user store between multiple applications is stupid. This probably does not affect you so go ahead and use it anyway. If for some reason you want multiple application support request it in an issue or add it yourself and send a pull request.

## How can I contribute ?

1. Fork this repo.
2. Write some tests.
3. Write your code.
4. Commit, push and send a pull request.

## Example

Create a new project using the ASP.NET MVC 4 Internet Application template.

Add the [nuget package](https://nuget.org/packages/BirdBrain) to your project.

Add a connection for BirdBrain to the **connectionStrings** section of your Web.config
```xml
<add name="RavenDB" connectionString="Url=http://localhost:8080;Database=BirdBrainMVC" providerName="Raven.Client.Document.DocumentStore"/>
```

Add the following to the **appSettings** section of your Web.config
```xml
<add key="enableSimpleMembership" value="false" />
```

Add the following to the **system.web** section of your Web.config
```xml
<membership defaultProvider="BirdBrainMembership">
    <providers>
    <clear />
    <add name="BirdBrainMembership" connectionStringName="RavenDB" applicationName="/" minRequiredPasswordLength="6" maxInvalidPasswordAttempts="5" minRequiredNonAlphanumericCharacters="0" passwordFormat="Hashed" passwordStrengthRegularExpression="[\d\w].*" requiresQuestionAndAnswer="true" type="BirdBrain.BirdBrainExtendedMembershipProvider, BirdBrain" />
    </providers>
</membership>
<roleManager defaultProvider="BirdBrainRole" enabled="true">
    <providers>
    <clear />
    <add name="BirdBrainRole" connectionStringName="RavenDB" applicationName="/" type="BirdBrain.BirdBrainRoleProvider, BirdBrain" />
    </providers>
</roleManager>
```