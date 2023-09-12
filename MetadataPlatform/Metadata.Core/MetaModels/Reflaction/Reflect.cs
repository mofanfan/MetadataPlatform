using System.Linq.Expressions;
using System.Reflection;

namespace MetaModels.Reflaction;

public static class Reflect<T>
{
    public static PropertyInfo GetPropertyInfo<TValue>(Expression<Func<T, TValue>> propertyExpr)
    {
        var info = GetMemberInfo(propertyExpr) as PropertyInfo;
        if (info == null) {
            throw new ArgumentException("Member is not a property");
        }

        return info;
    }

    private static MemberInfo GetMemberInfo(LambdaExpression expression)
    {
        if (expression == null) {
            throw new ArgumentNullException(nameof(expression));
        }

        MemberExpression? memberExpr = null;

        if (expression.Body.NodeType == ExpressionType.Convert) {
            memberExpr = ((UnaryExpression)expression.Body).Operand as MemberExpression;
        } else if (expression.Body.NodeType == ExpressionType.MemberAccess) {
            memberExpr = expression.Body as MemberExpression;
        }

        if (memberExpr == null) {
            throw new ArgumentException("Not a member access", nameof(expression));
        }

        return memberExpr.Member;
    }
}
