using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommanderDemo.Test
{
    /// <summary>
    /// FakeDbSet lets us test EntityFramework by mocking the virtual IDbSet collections defined
    /// for each table (thanks Brent!, see url below :)
    /// </summary>
    public static class FakeDbSetExtensions
    {
        public static IDbSet<T> ToDbSet<T>(this IEnumerable<T> items) where T : class
        {
            return new FakeDbSet<T>(items);
        }
    };

    //REF: http://blog.brentmckendrick.com/generic-repository-fake-idbset-implementation-update-find-method-identity-key/
    public class FakeDbSet<T> : IDbSet<T> where T : class
    {
        private readonly HashSet<T> _data;
        private readonly IQueryable _query;
        private int _identity;
        private List<PropertyInfo> _keyProperties;

        private void GetKeyProperties()
        {
            _keyProperties = new List<PropertyInfo>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                foreach (Attribute attribute in property.GetCustomAttributes(true))
                {
                    if (attribute is KeyAttribute)
                    {
                        _keyProperties.Add(property);
                    }
                }
            }
        }

        private void GenerateId(T entity)
        {
            // If non-composite integer key
            if (_keyProperties.Count == 1 && _keyProperties[0].PropertyType == typeof(int))
                _keyProperties[0].SetValue(entity, _identity++, null);
        }

        public FakeDbSet(IEnumerable<T> startData = null)
        {
            GetKeyProperties();
            _data = (startData != null ? new HashSet<T>(startData) : new HashSet<T>());
            _query = _data.AsQueryable();
            _identity = GetMaxId() + 1;
        }

        public virtual T Find(params object[] keyValues)
        {
            if (keyValues.Length != _keyProperties.Count)
                throw new ArgumentException("Incorrect number of keys passed to find method");

            var keyQuery = this.AsQueryable();
            for (var i = 0; i < keyValues.Length; i++)
            {
                var x = i; // nested linq
                keyQuery = keyQuery.Where(entity => _keyProperties[x].GetValue(entity, null).Equals(keyValues[x]));
            }

            return keyQuery.SingleOrDefault();
        }

        private int GetMaxId()
        {
            if (_keyProperties.Count != 1 || _keyProperties[0].PropertyType != typeof(int) || _data.Count == 0)
                return 0;

            var entity = _data.Last();
            return (int)_keyProperties[0].GetValue(entity);
        }

        public virtual T Add(T item)
        {
            GenerateId(item);
            _data.Add(item);
            return item;
        }

        public virtual T Remove(T item)
        {
            _data.Remove(item);
            return item;
        }

        public virtual T Attach(T item)
        {
            _data.Add(item);
            return item;
        }

        public virtual void Detach(T item)
        {
            _data.Remove(item);
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public virtual T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual ObservableCollection<T> Local
        {
            get { return new ObservableCollection<T>(_data); }
        }

        public virtual TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }
    };
}
