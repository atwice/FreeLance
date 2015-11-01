<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.web>
      <authentication mode="Forms">
          <forms name=".ADAuthCookie" loginUrl="~/Account/Login" timeout="45" slidingExpiration="false" protection="All" />
      </authentication>
      <membership defaultProvider="ADMembershipProvider">
          <providers>
              <clear />
              <add name="ADMembershipProvider" type="System.Web.Security.ActiveDirectoryMembershipProvider" connectionStringName="ADConnectionString" attributeMapUsername="sAMAccountName" />
          </providers>
      </membership>
  </system.web>
  <connectionStrings>
      <add name="ADConnectionString" connectionString="LDAP://primary.mydomain.local:389/DC=MyDomain,DC=Local" />
  </connectionStrings>
</configuration>