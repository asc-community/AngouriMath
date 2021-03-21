#define AM_PATH "D:\\main\\vs_prj\\AngouriMath\\AngouriMath\\Sources\\Wrappers\\AngouriMath.CPP.Exporting\\bin\\x64\\release\\netstandard2.0\\win-x64\\publish\\AngouriMath.CPP.Exporting.dll"
#include "utils.h"

namespace imported
{
	typedef ErrorCode(*ee2e)(EntityRef, EntityRef, EntityRef&);
	typedef ErrorCode(*s2e)(char*, EntityRef&);
	typedef ErrorCode(*e2s)(EntityRef, char*&);
	typedef ErrorCode(*e2)(EntityRef);

	auto differentiate = (ee2e)am_utils::import(AM_PATH, "diff");
	auto to_string = (e2s)am_utils::import(AM_PATH, "entity_to_string");
	auto parse = (s2e)am_utils::import(AM_PATH, "parse");
}
