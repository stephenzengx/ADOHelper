using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Helper
{
    public class Where<T>
    {
        private Expression<Func<T, bool>> _expressions = e => true;

        public void And(Expression<Func<T, bool>> exp)
        {
            _expressions = _expressions.AndAlso(exp, Expression.AndAlso);
        }

        public void Or(Expression<Func<T, bool>> exp)
        {
            _expressions = _expressions.AndAlso(exp, Expression.OrElse);
        }

        public Expression<Func<T, bool>> GetExpression()
        {
            return _expressions;
        }
    }
}
