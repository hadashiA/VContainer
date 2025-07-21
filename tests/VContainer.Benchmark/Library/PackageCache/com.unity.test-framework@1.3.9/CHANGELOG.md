# Changelog
### [1.3.9] - 2023-08-21
- Removed player metada fields that were using obsolete APIs (DSTR-880).
- Added note to documentation on mitigation of problem reported in (DSTR-600).
- Fixed an issue where the test runner ui causes progress bars to flicker or not show at all (DSTR-828). 

### [1.3.8] - 2023-07-05
- Send new UTP messages regarding player and system settings (DSTR-831)

### [1.3.7] - 2023-06-07
- The UTF version now automatically updates for SRP tests 

## [1.3.6] - 2023-06-01
- By using the editor command line new argument `-randomOrderSeed x` you can run the tests in a randomized order, where x is an integer different from 0. If a new test is added in the project the random order passing the same seed will be kept, and the new test will be placed in the random list accordigly.
- Fix for WebGL platform target to close the browser tab when the run is completed. 
- Added TestFileReferences.json to be generated on build step of the player, so can be consumed later by Test runners to enrich data for run part.

## [1.3.5] - 2023-05-16
- It’s now possible to retry and repeat tests on test level, meaning as soon as the test finishs running the first iteration, we now retry or repeat it.  Command line arguments to pass to the Editor:
  -  `-repeat x` runs the test x amount of times or until it fails. It is useful for testing unstable tests 
  -  `-retry x` if a test fails, run that test x amount of times or until it succeeds.
- Fixed various documentation bugs reported via the docs user feedback system.
- Added separate reference docs page for TestSettings options to distinguish them from regular command line arguments.
- Fixed TestMode not being set correctly on root level of test tree (DSTP-674).
- It's now possible to select browser for running WebGL player tests in player settings. (DSTR-811)

## [1.3.4] - 2023-03-24
- Fixes output message concurrency issue with async Setup.
- Fixed multiple issues where tests would not time out, when running longer than the default timeout or the timeout defined in a TimeoutAttribute (DSTR-607).  
- Added `UNITY_TEST_FRAMEWORK` define constraint to filter out test framework assemblies from normal platform and asset bundle builds. (DSTR-791)
- Ensured that all playmode tests have a disabled splashscreen and unity logo by default if Unity license permits such action.
- Added strictDomainReload feature to enable cheching for pending domain reloads/compilations at the end of managed tests (DSTR-793).

## [1.3.3] - 2023-02-10
- Fixes an issue where a test body would be skipped under certain conditions regarding domain reload.
- Fixed an issue where the "uncategorized" category filter would not apply correctly to parameterized tests with a category in the fixture (DSTR-700).
- Ensured that all samples can be loaded at once without assembly name collisions.

## [1.3.2] - 2022-12-07
- Fixed context not being restored after a domain reload outside tests (DSTR-678)
- Fixed TestMode being set only in on the aseembly level (DSTP-674)
- Fixed an issue where RunFinished callbacks sometimes would not be executed before the editor quits in batchmode (DSTR-692).
- Fixed problem of samples not loading for import in Package Manager window. (DSTR-702)
- Fixed issue GuiHelper depending on FilePath being abosolute. Updated to handle both cases.
- Fixed an issue where ITestRunCallback are invoked double when run in EditMode.

## [1.3.1] - 2022-10-18
- Fixed an issue where TestFinished sometimes causes failures when receiving fixture test results from a player (internal).

## [1.3.0] - 2022-10-11
- Fixed Xcode not closing after building iOS/tvOS project via batchmode `-runTests` command (ANT-679).
- Added TestSettings file options for setting `Target SDK` for iOS/tvOS (ANT-132).
- Async test support with documentation and support for SetUp and TearDown.
- Compute and share OneTimeSetup and OneTimeTearDown durations, these will be visible in the XML result under outputs (DSTR-597).
- Made test method/fixture arguments available in the ITestAdaptor as the `Arguments` property (DSTR-592).
- Added Learn Unity Test Framework section of documentation and related project files as importable package samples (DOCES-558).
- Fix NullReferenceException when yielding EditMode intructions in PlayMode tests (DSTR-622).

## [1.1.33] - 2022-07-12
- Fixed an issue where using Assert.Expect with the same string multiple times can lead to incorrect errors in some cases (DSTR-442).
- Improved the logging when using multiple Assert.Expect that the logs appear in another order than expected (DSTR-442).
- Moved the targetPlatform specified when running tests in the TestRunnerApi from the Filter to the ExecutionSettings (DSTR-186).
- Fixed an issue where an inheritance of UnityPlatformAttribute which was not working (ESTT-70).
- Fixed the log of excluded platforms which was not displaying the right information.
- Added filename and linenumber to test finished message (DSTR-505).
- Add the possibility of running tests in a specified order from a test list (DSTR-494).

## [1.1.32] - 2022-04-06
- Ensured that BuildTargetGroup is set correctly before TestPlayerBuildModifier is invoked (DSTR-394).
- Added a TestSetting that allows to build an Android App Bundle instead of APK.

## [1.1.31] - 2022-02-03
- Fixed "Open source code" on tests when located inside a package.
- Added editor analytics events.
- Added `buildPlayerPath` argument. Path to where built player with tests is saved.

## [1.1.30] - 2021-10-15
- Added validation of IEnumerator return type for parameterized tests with UnityTest attribute (DSTP-743).
- Fixed runInBackground reset to original value after finishing to run playmode tests (DSTR-248).
- Fixed issue with circular assembly references when constructing the test tree (DSTR-300).

## [1.1.29] - 2021-08-12
- Nested enumerator execution order fix (DSTR-227).
- Fix UI not running any tests if run select on a nested namespaces (DSTR-256).

## [1.1.28] - 2021-06-25
- Fix CountDownEvent reference due to `com.unity.ext.nunit` update.
- Various performance optimization to fix "Test execution timed out. No activity received from the player in 600 seconds."(DSTR-100).

## [1.1.27] - 2021-06-15
- Fix empty reason on passed tests results xml (DSTR-63)
- Fix Repeat and Retry attribute for UnityTest in PlayMode (DSTR-237).
- Remove XDK Xbox One platform after Unity 2020.3 
- Fixed issue when `.` suffix was applied to BuildTargets without extension.
- Added support for `GameCoreXboxOne` and `GameCoreXboxSeries` reduced location path length.

## [1.1.26] - 2021-05-25
- Fix html bug in TestRunnerApi API code snippet (DS-1973).
- Fix typo bug in PreBuildSetup code example (DS-1974).
- Fix incorrect syntax in command line reference (DS-1971).
- Fixed a bug where test filter would match project or player path (DSTP-412).
- Added playerGraphicsAPI TestSettings parameter
  
## [1.1.25] - 2021-05-05
- Fixed a bug where test filter would match project or player path (DSTP-412).
- Added playerGraphicsAPI TestSettings parameter

## [1.1.24] - 2021-03-04
- Improving UTF documentation(DSTR-120)
  - Updated "Actions outside of tests" section of user manual. Added flow charts to clarify execution order for SetUp/TearDown, TestActions, and complete flow (DSTR-121).
  - Fixed accepted values for scriptingBackend argument to be string literals instead of int values (DSTR-122).
  - Fixed possible values of ResultState to be Passed, Failed, Skipped, Inconclusive, plus labels instead of Success and Failure (DSTR-125).
  - Added NUNit version information (DSTR-130).
  - Added namespace information for LogAsset in user manual (DSTR-124).
  - Added instructions for creating additional sets of tests (DSTR-129).
  - Added information on testResults XML output format and exit codes (DSTR-131).
  - Updated description of testPlatform command line argument to clarify accepted values and their meaning (DSTR-123).
- Reduce time taken by filtering operations when only a subset of tests is run.
- Reduced the time taken to rebuild the test tree and to scan for assets a test created but did not delete.
- Reduce the per-test overhead of running tests in the editor.
- Added profiler markers around test setup, teardown, and execution.
- Fixed unstable timeout bug (DSTR-21).

## [1.1.23] - 2021-01-21
- Improving UTF documentation(DSTR-120)
  - Updated "Actions outside of tests" section of user manual. Added flow charts to clarify execution order for SetUp/TearDown, TestActions, and complete flow (DSTR-121).
  - Fixed accepted values for scriptingBackend argument to be string literals instead of int values (DSTR-122).
  - Fixed possible values of ResultState to be Passed, Failed, Skipped, Inconclusive, plus labels instead of Success and Failure (DSTR-125).
  - Added NUNit version information (DSTR-130).
  - Added namespace information for LogAsset in user manual (DSTR-124).
  - Added instructions for creating additional sets of tests (DSTR-129).
  - Added information on testResults XML output format and exit codes (DSTR-131).
  - Updated description of testPlatform command line argument to clarify accepted values and their meaning (DSTR-123).
  
## [1.1.22] - 2021-01-21
- Fixed issue where test result of an explicit test was set to skipped in case it was passing and running from command line with testfilter set to the explicit test (DS-1236).
- Fixed an issue where tests located in assemblies that did not directly reference any test assemblies were not included (DSTR-30).
- Fixed an issue where UnitySetup methods were incorrectly being rerun when entering playmode, rather than being skipped (DSTR-68).
- Internal: Remove ##utp message AssemblyCompilationErrors (DS-1277)
- Fixed issue where if the timeout was exceeded in SetUp the timeout exception was not thrown(DSTR-21).
- Removed ability to `Enable playmode tests for all assemblies` from the TestRunner UI, since it is a deprecated behavior. It enforces to use of assembly definition files (DSTR-45).
- Fixed typo in `LogAssert.cs` documentation.

## [1.1.21] - 2020-12-04
- Fixed issue where test result of an explicit test was set to skipped in case it was passing and running from command line with testfilter set to the explicit test (DS-1236).
- Fixed an issue where tests located in assemblies that did not directly reference any test assemblies were not included (DSTR-30).
- Fixed an issue where UnitySetup methods were incorrectly being rerun when entering playmode, rather than being skipped (DSTR-68).
- Internal: Remove ##utp message AssemblyCompilationErrors (ds-1277)
- Fixed issue where if the timeout was exceeded in SetUp the timeout exception was not thrown(DSTR-21).
- Removed ability to `Enable playmode tests for all assemblies` from the TestRunner UI, since it is a deprecated behavior. It enforces to use of assembly definition files (DSTR-45).

## [1.1.20] - 2020-12-04
- The logscope is now available in OneTimeTearDown.
- Fixed an issue where failing tests would not result in the correct exit code if a domain reload happens after the test has run (DS-1304).
- If a player build fails, the test specific build settings should be cleaned up and the original values restored as intended (DS-1001).
- Added better error message when using TestRunCallbackAttribute and the implementation is stripped away (DS-454).
- Fixed an issue where the test results xml would have a zero end-time for tests executed before a domain reload (DSTR-63).
- Fixed OpenSource in case of a Test in a nested class (DSTR-6)
- UnityTests with a domain reload now works correctly in combination with Retry and Repeat attributes (DS-428).
- Fixed OpenSource in case of Tests located inside a package (DS-432)

## [1.1.19] - 2020-11-17
- Command line runs with an inconclusive test result now exit with exit code 2 (case DS-951).
- Fixed timeout during UnitySetUp which caoused test to pass instead of failing due to wrong time format.
- Timeout exeption thrown when timeout time is exeded in the UnitySetup when using `WaitForSeconds(n)`.
- Updating `com.unity.ext.nunit` version
- Method marked with UnityTest that are not returning IEnumerator is now giving a proper error (DS-1059).

## [1.1.18] - 2020-10-07
- Fixed issue of timeout during UnitySetUp which wasn't detected and allowed the test to pass instead of failing (case DSTR-21)

## [1.1.17] - 2020-10-05
- Fixed an issue where the WaitForDomainReload yield instruction would sometimes let the test continue for one frame before the domain reload.
- Added support for negation in filters using !. E.g. !CategoryToExclude.
- Fixed an issue where if the first test enters PlayMode from UnitySetup then the test body will not run on consecutive runs (case 1260901). 
- Clear Results button clears the test results in the GUI (DSTR-16)
- Improved UI in Test Runner window, added new options:
	- Run Selected Tests in player
	- Build/Export project with all tests in player
	- Build/Export project with selected tests in player
- Fixed issue on loading EditMode or Playmode test tree in the wrong tab when switching between tabs when TestRunner is loading (DS-865)

## [1.1.16] - 2020-07-09
- Follow up on fix when UTF picks up on outdated compilation errors

## [1.1.15] - 2020-07-02
- Fixed an issue where an exception is thrown on getting the enumerator of a UnityTest would result in stopping the test run instead of failing it (case 1212000).
- Including a trailing semi-colon in a testName filter no longer results in all tests being run (case 1171200).
- Fixed and issue when Unity Test Framework exits editor on an outdated script compilation error (during api updates)

## [1.1.14] - 2020-04-03
- Added the 'assemblyNames' command line argument for filtering on the assembly level.
- The dll and project level of the tree view should now correctly show the results when running tests in a player (case 1197026).
- Optimize usage of player connection when transfering test results (case 1229200).
- Ignore internal test framework tests assertions (case 1206961).

## [1.1.13] - 2020-03-16
- Fixed an issue where a combination of Entering / Exiting playmode and recompiling scripts would result in the test run repeating (case 1213958).
- Fixed a regression from 1.1.12 where prefabs left in the scene would be cleaned up to aggressively.
- Fixed Test execution timed out. No activity received from the player in 600 seconds error when player is not supposed to start (case 1225147)

## [1.1.12] - 2020-03-02
- Now 'Open error line' for a failed UTF test does not throw exceptions for corrupted testable pdb in Editor release mode (case 1118259)
- Fixed an issue where running a test fixture would also run other fixtures with the same full name (namespace plus classname) in other assemblies (case 1197385).
- Running tests with the same full name, with a domain reload inbetween, will no longer fail to initialize the fixture of the second class (case 1205240).
- Running a playmode tests with "Maximize on Play" will now correctly show the result of the tests in the test runner window (case 1014908).
- Fixed an issue where leaving a game object in a scene with a DontSaveInEditor hideFlags would result in an error on cleanup (case 1136883).
- Now ITestPlayerBuildModifier.ModifyOptions is called as expected when running tests on a device (case 1213845)

## [1.1.11] - 2020-01-16
- Fixed test runner dlls got included into player build (case 1211624)
- Passing a non-full-path of XML file for -testResults in Unity Batchmode issue resolved, now passing "result.xml" creates the result file in the project file directory (case 959078)
- Respect Script Debugging build setting when running tests

## [1.1.10] - 2019-12-19
- Introduced PostSuccessfulLaunchAction callback
- Fixed an issue where canceling a UnityTest while it was running would incorrectly mark it as passed instead of canceled.
- Added command line argument for running tests synchronously.
- The test search bar now handles null values correctly.
- The test output pane now retains its size on domain reloads.

## [1.1.9] - 2019-12-12
- Rolled back refactoring to the test run system, as it caused issues in some corner cases.

## [1.1.8] - 2019-11-15
- Ensured that a resumed test run is continued instantly. 

## [1.1.7] - 2019-11-14
- Fixed an issue with test runs after domain reload.

## [1.1.6] - 2019-11-12
- Building a player for test will no longer look in unrelated assemblies for relevant attributes.

## [1.1.5] - 2019-10-23
- Fixed a regression to synchronous runs introduced in 1.1.4.

## [1.1.4] - 2019-10-15
- Running tests in batch mode now correctly returns error code 3 (RunError) when a timeout or a build error occurs.
- Fixed an issue where a test run in a player would time out, if the player takes longer than 10 minutes to run.
- Added command line argument and api setting for specifying custom heartbeat timeout for running on players.

## [1.1.3] - 2019-09-23
- Fixed a regression where tests in a player would report a timeout after a test run is finished.
- Made it possible for the ui to change its test items when the test tree changes without script compilation.
- Added synchronous runs as an option to the TestRunnerApi.

## [1.1.2] - 2019-09-11
- Fixed an issue where Run Selected would run all tests in the category, if a category filter was selected, regardless of what tests were selected.
- Unsupported attributes used in UnityTests now give an explicit error.
- Added support for the Repeat and Retry attributes in UnityTests (case 1131940).
- Tests with a explicit timeout higher than 10 minutes, no longer times out after running longer than 10 minutes when running from command line (case 1125991).
- Fixed a performance regression in the test runner api result reporting, introduced in 2018.3 (case 1109865).
- Fixed an issue where parameterized test fixtures would not run if selected in the test tree (case 1092244).
- Pressing Clear Results now also correctly clears the counters on the test list (case 1181763).
- Prebuild setup now handles errors logged with Debug.LogError and stops the run if any is logged (case 1115240). It now also supports LogAssert.Expect.

## [1.1.1] - 2019-08-07
- Tests retrieved as a test list with the test runner api incorrectly showed both mode as their TestMode.
- Fixed a compatibility issue with running tests from rider.

## [1.1.0] - 2019-07-30
- Introduced the TestRunnerApi for running tests programmatically from elsewhere inside the Editor.
- Introduced yield instructions for recompiling scripts and awaiting a domain reload in Edit Mode tests.
- Added a button to the Test Runner UI for clearing the results.

## [1.0.18] - 2019-07-15
- Included new full documentation of the test framework.

## [1.0.17] - 2019-07-11
- Fixed an issue where the Test Runner window wouldn’t frame selected items after search filter is cleared.
- Fixed a regression where playmode test application on the IOS platform would not quit after the tests are done.

## [1.0.16] - 2019-06-20
- Fixed an issue where the Test Runner window popped out if it was docked, or if something else was docked next to it, when re-opened (case 1158961)
- Fixed a regression where the running standalone playmode tests from the ui would result in an error.

## [1.0.15] - 2019-06-18
- Added new `[TestMustExpectAllLogs]` attribute, which automatically does `LogAssert.NoUnexpectedReceived()` at the end of affected tests. See docs for this attribute for more info on usage.
- Fixed a regression where no tests would be run if multiple filters are specified. E.g. selecting both a whole assembly and an individual test in the ui.
- Fixed an issue where performing `Run Selected` on a selected assembly would run all assemblies.
- Introduced the capability to do a split build and run, when running playmode tests on standalone devices.
- Fixed an error in ConditionalIgnore, if the condition were not set.

## [1.0.14] - 2019-05-27
- Fixed issue preventing scene creation in IPrebuildSetup.Setup callback when running standalone playmode tests.
- Fixed an issue where test assemblies would sometimes not be ordered alphabetically.
- Added module references to the package for the required modules: imgui and jsonserialize.
- Added a ConditionalIgnore attribute to help ignoring tests only under specific conditions.
- Fixed a typo in the player test window (case 1148671).

## [1.0.13] - 2019-05-07
- Fixed a regression where results from the player would no longer update correctly in the UI (case 1151147).

## [1.0.12] - 2019-04-16
- Added specific unity release to the package information.

## [1.0.11] - 2019-04-10
- Fixed a regression from 1.0.10 where test-started events were triggered multiple times after a domain reload.

## [1.0.10] - 2019-04-08
- Fixed an issue where test-started events would not be fired correctly after a test performing a domain reload (case 1141530).
- The UI should correctly run tests inside a nested class, when that class is selected.
- All actions should now correctly display a prefix when reporting test result. E.g. "TearDown :".
- Errors logged with Debug.LogError in TearDowns now append the error, rather than overwriting the existing result (case 1114306).
- Incorrect implementations of IWrapTestMethod and IWrapSetUpTearDown now gives a meaningful error.
- Fixed a regression where the Test Framework would run TearDown in a base class before the inheriting class (case 1142553).
- Fixed a regression introduced in 1.0.9 where tests with the Explicit attribute could no longer be executed.

## [1.0.9] - 2019-03-27
- Fixed an issue where a corrupt instance of the test runner window would block for a new being opened.
- Added the required modules to the list of package requirements.
- Fixed an issue where errors would happen if the test filter ui was clicked before the ui is done loading.
- Fix selecting items with duplicate names in test hierarchy of Test Runner window (case 987587).
- Fixed RecompileScripts instruction which we use in tests (case 1128994).
- Fixed an issue where using multiple filters on tests would sometimes give an incorrect result.

## [1.0.7] - 2019-03-12
### This is the first release of *Unity Package com.unity.test-framework*.

- Migrated the test-framework from the current extension in unity.
