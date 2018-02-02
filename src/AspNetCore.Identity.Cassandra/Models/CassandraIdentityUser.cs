﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cassandra.Mapping.Attributes;

namespace AspNetCore.Identity.Cassandra.Models
{
    public class CassandraIdentityUser
    {
        #region | Fields

        private readonly List<LoginInfo> _logins;
        private readonly List<TokenInfo> _tokens;
        private readonly List<string> _roles;

        #endregion

        #region | Properties

        [PartitionKey]
        public Guid Id { get; internal set; }

        public string UserName { get; set; }
        public string NormalizedUserName { get; internal set; }
        public string NormalizedEmail { get; set; }

        public string Email { get; set; }
        public DateTimeOffset? EmailConfirmationTime { get; set; }

        public string PasswordHash { get; internal set; }
        public bool TwoFactorEnabled { get; internal set; }
        public string SecurityStamp { get; internal set; }

        [Frozen]
        public LockoutInfo Lockout { get; internal set; }

        [Frozen]
        public PhoneInfo Phone { get; internal set; }

        [Frozen]
        public IEnumerable<LoginInfo> Logins
        {
            get => _logins;
            internal set
            {
                if (value != null)
                    _logins.AddRange(value);
            }
        }

        [Frozen]
        public IEnumerable<TokenInfo> Tokens
        {
            get => _tokens;
            internal set
            {
                if (value != null)
                    _tokens.AddRange(value);
            }
        }

        [SecondaryIndex]
        public IEnumerable<string> Roles
        {
            get => _roles;
            internal set
            {
                if (value != null)
                    _roles.AddRange(value);
            }
        }

        [Ignore]
        public bool EmailConfirmed => EmailConfirmationTime.HasValue;

        #endregion

        #region | Constructors

        public CassandraIdentityUser()
        {
            _logins = new List<LoginInfo>();
            _tokens = new List<TokenInfo>();
            _roles = new List<string>();
        }

        public CassandraIdentityUser(Guid id)
            : this()
        {
            Id = id;
        }

        #endregion

        #region | Internal Methods

        internal void CleanUp()
        {
            if (Lockout != null && Lockout.AllPropertiesAreSetToDefaults)
                Lockout = null;

            if (Phone != null && Phone.AllPropertiesAreSetToDefaults)
                Phone = null;
        }

        internal void AddLogin(LoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            if (_logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
                throw new InvalidOperationException($"Login with LoginProvider: '{login.LoginProvider}' and ProviderKey: {login.ProviderKey} already exists.");

            _logins.Add(login);
        }

        internal void RemoveLogin(string loginProvider, string providerKey)
        {
            var loginToRemove = _logins.FirstOrDefault(l =>
                l.LoginProvider == loginProvider &&
                l.ProviderKey == providerKey
            );

            if (loginToRemove == null)
                return;

            _logins.Remove(loginToRemove);
        }

        internal void AddToken(TokenInfo token)
        {
            if(token == null)
                throw new ArgumentNullException(nameof(token));

            if(_tokens.Any(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name))
                throw new InvalidOperationException($"Token with LoginProvider: '{token.LoginProvider}' and Name: {token.Name} already exists.");

            _tokens.Add(token);
        }

        internal void RemoveToken(string loginProvider, string name)
        {
            var tokenToRemove = _tokens.FirstOrDefault(l =>
                l.LoginProvider == loginProvider &&
                l.Name == name
            );

            if (tokenToRemove == null)
                return;

            _tokens.Remove(tokenToRemove);
        }

        internal void AddRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(role));

            if (!_roles.Contains(role))
                _roles.Add(role);
        }

        internal void RemoveRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(role));

            if (_roles.Contains(role))
                _roles.Remove(role);
        }
        
        #endregion
    }
}