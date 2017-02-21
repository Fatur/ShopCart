
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;

namespace Login
{
    public class Config
    {
        internal static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                new Client()
                {
                    ClientId = "gateway",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "ap1" }
                },
                new Client() {
                     ClientId = "openIdConnectClient",
                     ClientName = "Example Implicit Client Application",
                     AllowedGrantTypes = GrantTypes.Implicit,
                     AllowedScopes = new List<string>
                        {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                IdentityServerConstants.StandardScopes.Email,
                                "role",
                                "ap1"
                },
                RedirectUris = new List<string> {"http://localhost:5000/signin-oidc"},
                PostLogoutRedirectUris = new List<string> {"http://localhost:5000"}
                }
            };
        
        }

        internal static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("ap1","Shopping Cart Services")
                
            };
        }

        internal static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                }
            };
        }

        internal static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };
        }
    }
}
