﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;

namespace Microsoft.Data.SqlClient.ManualTesting.Tests
{
    public class AADConnectionsTest
    {
        private static void ConnectAndDisconnect(string connectionString, SqlCredential credential = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (credential != null)
                {
                    conn.Credential = credential;
                }
                conn.Open();
            }
        }

        private static bool IsAccessTokenSetup() => DataTestUtility.IsAccessTokenSetup();
        private static bool IsAADConnStringsSetup() => DataTestUtility.IsAADPasswordConnStrSetup();

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AccessTokenTest()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "User ID", "Password", "UID", "PWD", "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.AccessToken = DataTestUtility.GetAccessToken();
                connection.Open();
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void InvalidAccessTokenTest()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "User ID", "Password", "UID", "PWD", "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.AccessToken = DataTestUtility.GetAccessToken() + "abc";
                SqlException e = Assert.Throws<SqlException>(() => connection.Open());

                string expectedMessage = "Login failed for user";
                Assert.Contains(expectedMessage, e.Message);
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AccessTokenWithAuthType()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "User ID", "Password", "UID", "PWD" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                InvalidOperationException e = Assert.Throws<InvalidOperationException>(() =>
                    connection.AccessToken = DataTestUtility.GetAccessToken());

                string expectedMessage = "Cannot set the AccessToken property if 'Authentication' has been specified in the connection string.";
                Assert.Contains(expectedMessage, e.Message);
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AccessTokenWithCred()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                InvalidOperationException e = Assert.Throws<InvalidOperationException>(() =>
                connection.AccessToken = DataTestUtility.GetAccessToken());

                string expectedMessage = "Cannot set the AccessToken property if 'UserID', 'UID', 'Password', or 'PWD' has been specified in connection string.";
                Assert.Contains(expectedMessage, e.Message);
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AccessTokenTestWithEmptyToken()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "User ID", "Password", "UID", "PWD", "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.AccessToken = "";
                SqlException e = Assert.Throws<SqlException>(() => connection.Open());

                string expectedMessage = "A connection was successfully established with the server, but then an error occurred during the login process.";
                Assert.Contains(expectedMessage, e.Message);
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AccessTokenTestWithIntegratedSecurityTrue()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "User ID", "Password", "UID", "PWD", "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys) + "Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                InvalidOperationException e = Assert.Throws<InvalidOperationException>(() => connection.AccessToken = "");

                string expectedMessage = "Cannot set the AccessToken property if the 'Integrated Security' connection string keyword has been set to 'true' or 'SSPI'.";
                Assert.Contains(expectedMessage, e.Message);
            }
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void InvalidAuthTypeTest()
        {
            // Remove cred info and add invalid token
            string[] credKeys = { "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys) + "Authentication=Active Directory Pass;";

            ArgumentException e = Assert.Throws<ArgumentException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Invalid value for key 'authentication'.";
            Assert.Contains(expectedMessage, e.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void AADPasswordWithIntegratedSecurityTrue()
        {
            string connStr = DataTestUtility.AADPasswordConnectionString + "Integrated Security=True;";

            ArgumentException e = Assert.Throws<ArgumentException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Cannot use 'Authentication' with 'Integrated Security'.";
            Assert.Contains(expectedMessage, e.Message);
        }

        [ConditionalFact(nameof(IsAccessTokenSetup), nameof(IsAADConnStringsSetup))]
        public static void AADPasswordWithWrongPassword()
        {
            string[] credKeys = { "Password", "PWD" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys) + "Password=TestPassword;";

            AggregateException e = Assert.Throws<AggregateException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "ID3242: The security token could not be authenticated or authorized.";
            Assert.Contains(expectedMessage, e.InnerException.InnerException.InnerException.Message);
        }


        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void GetAccessTokenByPasswordTest()
        {
            using (SqlConnection connection = new SqlConnection(DataTestUtility.AADPasswordConnectionString))
            {
                connection.Open();
            }
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void testADPasswordAuthentication()
        {
            // Connect to Azure DB with password and retrieve user name.
            try
            {
                using (SqlConnection conn = new SqlConnection(DataTestUtility.AADPasswordConnectionString))
                {
                    conn.Open();
                    using (SqlCommand sqlCommand = new SqlCommand
                    (
                        cmdText: $"SELECT SUSER_SNAME();",
                        connection: conn,
                        transaction: null
                    ))
                    {
                        string customerId = (string)sqlCommand.ExecuteScalar();
                        string expected = DataTestUtility.RetrieveValueFromConnStr(DataTestUtility.AADPasswordConnectionString, new string[] { "User ID", "UID" });
                        Assert.Equal(expected, customerId);
                    }
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void ActiveDirectoryPasswordWithNoAuthType()
        {
            // connection fails with expected error message.
            string[] AuthKey = { "Authentication" };
            string connStrWithNoAuthType = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, AuthKey);
            SqlException e = Assert.Throws<SqlException>(() => ConnectAndDisconnect(connStrWithNoAuthType));

            string expectedMessage = "Cannot open server \"microsoft.com\" requested by the login.  The login failed.";
            Assert.Contains(expectedMessage, e.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        [SkipOnTargetFramework(TargetFrameworkMonikers.Netcoreapp)]
        public static void IntegratedAuthWithCred()
        {
            // connection fails with expected error message.
            string[] AuthKey = { "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, AuthKey) + "Authentication=Active Directory Integrated;";
            ArgumentException e = Assert.Throws<ArgumentException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Cannot use 'Authentication=Active Directory Integrated' with 'User ID', 'UID', 'Password' or 'PWD' connection string keywords.";
            Assert.Contains(expectedMessage, e.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        [SkipOnTargetFramework(TargetFrameworkMonikers.Netcoreapp)]
        public static void MFAAuthWithCred()
        {
            // connection fails with expected error message.
            string[] AuthKey = { "Authentication" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, AuthKey) + "Authentication=Active Directory Interactive;";
            ArgumentException e = Assert.Throws<ArgumentException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Cannot use 'Authentication=Active Directory Interactive' with 'User ID', 'UID', 'Password' or 'PWD' connection string keywords.";
            Assert.Contains(expectedMessage, e.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void EmptyPasswordInConnStrAADPassword()
        {
            // connection fails with expected error message.
            string[] pwdKey = { "Password", "PWD" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, pwdKey) + "Password=;";
            AggregateException e = Assert.Throws<AggregateException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "ID3242: The security token could not be authenticated or authorized.";
            Assert.Contains(expectedMessage, e.InnerException.InnerException.InnerException.Message);
        }

        [SkipOnTargetFramework(TargetFrameworkMonikers.Netcoreapp)]
        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void EmptyCredInConnStrAADPasswordNetFx()
        {
            // connection fails with expected error message.
            string[] removeKeys = { "User ID", "Password", "UID", "PWD" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, removeKeys) + "User ID=; Password=;";
            AggregateException e = Assert.Throws<AggregateException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Failed to get user name";

            Assert.Contains(expectedMessage, e.InnerException.InnerException.InnerException.Message);
        }

        [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)]
        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void EmptyCredInConnStrAADPasswordNetCore()
        {
            // connection fails with expected error message.
            string[] removeKeys = { "User ID", "Password", "UID", "PWD" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, removeKeys) + "User ID=; Password=;";
            AggregateException e = Assert.Throws<AggregateException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "Could not identify the user logged into the OS";

            Assert.Contains(expectedMessage, e.InnerException.InnerException.InnerException.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void AADPasswordWithInvalidUser()
        {
            // connection fails with expected error message.
            string[] removeKeys = { "User ID", "UID" };
            string connStr = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, removeKeys) + "User ID=testdotnet@microsoft.com";
            AggregateException e = Assert.Throws<AggregateException>(() => ConnectAndDisconnect(connStr));

            string expectedMessage = "ID3242: The security token could not be authenticated or authorized.";
            Assert.Contains(expectedMessage, e.InnerException.InnerException.InnerException.Message);
        }

        [ConditionalFact(nameof(IsAADConnStringsSetup))]
        public static void NoCredentialsActiveDirectoryPassword()
        {
            // test Passes with correct connection string.
            ConnectAndDisconnect(DataTestUtility.AADPasswordConnectionString);

            // connection fails with expected error message.
            string[] credKeys = { "User ID", "Password", "UID", "PWD" };
            string connStrWithNoCred = DataTestUtility.RemoveKeysInConnStr(DataTestUtility.AADPasswordConnectionString, credKeys);
            InvalidOperationException e = Assert.Throws<InvalidOperationException>(() => ConnectAndDisconnect(connStrWithNoCred));

            string expectedMessage = "Either Credential or both 'User ID' and 'Password' (or 'UID' and 'PWD') connection string keywords must be specified, if 'Authentication=Active Directory Password'.";
            Assert.Contains(expectedMessage, e.Message);
        }
    }
}
