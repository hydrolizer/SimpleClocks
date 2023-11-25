using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleClocks.Utils.Extensions
{
	public enum ErrorFormat
	{
		Full,
		WithType,
		MessageOnly,
		Verbose
	}

	public static class ExceptionExtension
	{
		public static string Format(this Exception e, bool onlyMessage = false)
		{
			return onlyMessage ? e.Message : $"{e.GetType().Name}: {e.Message}{Environment.NewLine}{e.StackTrace}";
		}

		public static string Format(this Exception e, ErrorFormat format, bool oneLineTopMessage=false)
		{
			switch(format)
			{
				case ErrorFormat.MessageOnly:
					return e.Message;
				case ErrorFormat.WithType:
					return $"{e.GetType().Name}: {e.Message}";
				case ErrorFormat.Full:
					return $"{e.GetType().Name}: {e.Message}{Environment.NewLine}{e.StackTrace}";
				case ErrorFormat.Verbose:
					return GetExtendedInfo(e, oneLineTopMessage);
				default:
					return e.ToString();
			}
		}

		private static string GetExtendedInfo(Exception error, bool oneLineMessage)
		{
			var sb = new StringBuilder();
			var innerExceptionLevel = 0;
			if (!(error is AggregateException))
			{
				AppendExceptionInfo(error, sb, oneLineMessage);
				PrepareInnerException(error, sb, ref innerExceptionLevel);
			}
			if (error is TargetInvocationException reflectionException)
			{
				sb.AppendLine("TargetInvocationException.Base exception:");
				AppendExceptionInfo(reflectionException.GetBaseException(), sb, false);
				innerExceptionLevel = 0;
				PrepareInnerException(reflectionException.GetBaseException(), sb, ref innerExceptionLevel);
			}
			if (error is ReflectionTypeLoadException rtle)
			{
				sb.AppendLine("ReflectionTypeLoadException exceptions:");
				foreach(var e in rtle.LoaderExceptions)
					AppendExceptionInfo(e, sb, false);
			}
			if (error is AggregateException ae)
			{
				sb.AppendLine("AggregateException exceptions:");
				foreach(var e in ae.InnerExceptions)
					sb.AppendLine(GetExtendedInfo(e, false));
			}
			return sb.ToString();
		}

		private static void PrepareInnerException(Exception error, StringBuilder builder, ref int level)
		{
			if (error == null || error.InnerException == null) return;
			level++;
			builder.AppendLine($"Inner Exception ({level}):");
			AppendExceptionInfo(error.InnerException, builder, false);
			PrepareInnerException(error.InnerException, builder, ref level);
		}

		static readonly Regex rxLineBreak = new Regex(@"[\r\n]+", RegexOptions.Compiled);
		private static void AppendExceptionInfo(Exception error, StringBuilder builder, bool oneLineMessage)
		{
			if (error == null) return;
			var msg = oneLineMessage
				? rxLineBreak.Replace(error.Message, " ")
				: error.Message;
			builder.AppendLine($"{error.GetType().FullName}: {msg}");
			builder.AppendLine(error.StackTrace);
		}

		public static bool IsCriticalException(this Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));
			exception = Unwrap(exception);
			return
				exception is NullReferenceException ||
				exception is StackOverflowException ||
				exception is OutOfMemoryException ||
				exception is ThreadAbortException ||
				exception is SEHException ||
				exception is SecurityException;
		}

		public static bool IsCriticalApplicationException(this Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));
			exception = Unwrap(exception);
			return
				exception is StackOverflowException ||
				exception is OutOfMemoryException ||
				exception is ThreadAbortException ||
				exception is SecurityException;
		}

		public static Exception Unwrap(this Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));
			while (exception.InnerException != null && exception is TargetInvocationException)
			{
				exception = exception.InnerException;
			}
			return exception;
		}
	}
}
