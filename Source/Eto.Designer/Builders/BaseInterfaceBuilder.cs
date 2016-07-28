using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eto.Designer.Builders
{
	public abstract class BaseInterfaceBuilder : IInterfaceBuilder, IDisposable
	{
		AppDomain newDomain;
		string output;
		const string assemblyName = "Generated";


		public string InitializeAssembly { get; set; }
		protected string BaseDir { get; set; }

		protected BaseInterfaceBuilder(string baseDir = null)
		{
			BaseDir = baseDir ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		~BaseInterfaceBuilder()
		{
			Dispose(false);
		}

		void RemoveOutput()
		{
			if (!string.IsNullOrEmpty(output) && File.Exists(output))
			{
				File.Delete(output);
				output = null;
			}
		}

		void UnloadDomain()
		{
			if (newDomain != null && !newDomain.IsFinalizingForUnload())
			{
				AppDomain.Unload(newDomain);
				newDomain = null;
			}
		}

		public static Control InstantiateControl(Type type)
		{
			var control = Activator.CreateInstance(type) as Control;
			if (control != null)
			{
				var initializeMethod = control.GetType().GetMethod("InitializeComponent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, Type.DefaultBinder, Type.EmptyTypes, null);
				if (initializeMethod != null)
					initializeMethod.Invoke(control, null);
			}
			return control;
		}

		public static Type FindControlType(Assembly asm)
		{
			return asm.GetTypes().FirstOrDefault(t => typeof(Control).IsAssignableFrom(t));
		}

		public void Create(string text, Action<Control> controlCreated, Action<Exception> error)
		{
			UnloadDomain();
			RemoveOutput();

			ThreadPool.QueueUserWorkItem(state =>
			{
				try
				{
					bool useAppDomain = Platform.Instance.Supports<IEtoAdapterHandler>();

					output = useAppDomain ? Path.GetTempFileName() + ".dll" : null;

					Assembly asm;

					var result = Compile(output, text, out asm);
					if (result.Success)
					{
						Application.Instance.Invoke(() =>
						{
							try
							{
								Control control = null;
								if (useAppDomain)
								{
#pragma warning disable 618
									// doesn't work without for some reason, and there's no non-obsolete alternative.
									if (!AppDomain.CurrentDomain.ShadowCopyFiles)
									AppDomain.CurrentDomain.SetShadowCopyFiles();
#pragma warning restore 618

									var setup = new AppDomainSetup
									{
										ApplicationBase = BaseDir,
										PrivateBinPath = BaseDir,
											//PrivateBinPath = Path.GetDirectoryName(output) + ";" + BaseDir,
											ShadowCopyFiles = "true",
											ShadowCopyDirectories = BaseDir,
											//LoaderOptimization = LoaderOptimization.MultiDomainHost,
											LoaderOptimization = LoaderOptimization.NotSpecified
									};

									newDomain = AppDomain.CreateDomain("newDomain", null, setup);
									var module = newDomain.CreateInstanceFromAndUnwrap(typeof(ControlLoader).Assembly.Location, typeof(ControlLoader).FullName) as ControlLoader;
									if (module == null)
										throw new InvalidOperationException("Could not create ControlLoader instance in new domain");
										//var executeMethod = module.GetType().GetMethod("Execute");
										//var contract = executeMethod.Invoke(module, new object[] { Platform.Instance.GetType().FullName + ", " + Platform.Instance.GetType().Assembly.FullName, assemblyName, InitializeAssembly });
										var contract = module.Execute(Platform.Instance.GetType().FullName + ", " + Platform.Instance.GetType().Assembly.FullName, assemblyName, InitializeAssembly);
									if (contract != null)
										control = EtoAdapter.ToControl(contract);
								}
								else
								{
									using (AssemblyResolver.Register(BaseDir))
									{
										var type = FindControlType(asm);
										if (type != null)
											control = InstantiateControl(type);
									}
								}
								if (control != null)
									controlCreated(control);
								else
									error(new FormatException("Could not find control. Make sure you have a single class derived from Control."));
							}
							catch (Exception ex)
							{
								error(ex);
							}
						});
					}
					else
					{
						var errorText = string.Join("\n", result.Errors);
						Application.Instance.Invoke(() => error(new FormatException(string.Format("Compile error: {0}", errorText))));
					}
				}
				catch (Exception ex)
				{
					Application.Instance.Invoke(() => error(ex));
				}
			});
		}

		protected const string ReferenceAssembliesFolder = @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5";

		protected string GetReferenceAssembliesPath(string basePath)
		{
			if (string.IsNullOrEmpty(basePath))
				return null;
			var path = Path.Combine(basePath, ReferenceAssembliesFolder);
			return Directory.Exists(path) ? path : null;
		}

		public class CompileResult
		{
			public bool Success { get; set; }

			public IEnumerable<string> Errors { get; set; }
		}

		protected abstract CompileResult Compile(string output, string code, out Assembly generatedAssembly);

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnloadDomain();
				GC.SuppressFinalize(this);
			}
			RemoveOutput();
		}
	}
}
