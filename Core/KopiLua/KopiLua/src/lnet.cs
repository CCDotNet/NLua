using System;

namespace KopiLua
{
	public partial class Lua
	{
		public struct LuaTag
		{
			public LuaTag (object tag): this ()
			{
				this.Tag = tag;
			}
			
			public object Tag { get; set; }
		}

		private static object tag = 0;

		public static void lua_pushstdcallcfunction (lua_State luaState, lua_CFunction function)
		{
			lua_pushcfunction (luaState, function);
		}

		public static bool luaL_checkmetatable (lua_State luaState, int index)
		{
			bool retVal = false;
			
			if (lua_getmetatable (luaState, index) != 0) {
				lua_pushlightuserdata (luaState, tag);
				lua_rawget (luaState, -2);
				retVal = !lua_isnil (luaState, -1);
				lua_settop (luaState, -3);
			}
			
			return retVal;
		}

		public static LuaTag luanet_gettag ()
		{
			return new LuaTag (tag);
		}

		public static void lua_pushlightuserdata (lua_State L, LuaTag p)
		{
			lua_pushlightuserdata (L, p.Tag);
		}

		// Starting with 5.1 the auxlib version of checkudata throws an exception if the type isn't right
		// Instead, we want to run our own version that checks the type and just returns null for failure
		private static object checkudata_raw (lua_State L, int ud, string tname)
		{
			object p = lua_touserdata (L, ud);
			
			if (p != null) {
				/* value is a userdata? */
				if (lua_getmetatable (L, ud) != 0) { 
					bool isEqual;
					
					/* does it have a metatable? */
					lua_getfield (L, LUA_REGISTRYINDEX, tname);  /* get correct metatable */
					
					isEqual = lua_rawequal (L, -1, -2) != 0;
					
					// NASTY - we need our own version of the lua_pop macro
					// lua_pop(L, 2);  /* remove both metatables */
					lua_settop (L, -(2) - 1);
					
					if (isEqual)	/* does it have the correct mt? */
						return p;
				}
			}
			
			return null;
		}


		public static int luanet_checkudata (lua_State luaState, int ud, string tname)
		{
			object udata = checkudata_raw (luaState, ud, tname);
			return udata != null ? fourBytesToInt (udata as byte[]) : -1;
		}

		public static int luanet_tonetobject (lua_State luaState, int index)
		{
			byte[] udata;
			
			if (lua_type (luaState, index) == LUA_TUSERDATA) {
				if (luaL_checkmetatable (luaState, index)) {
					udata = lua_touserdata (luaState, index) as byte[];
					if (udata != null)
						return fourBytesToInt (udata);
				}
				
				udata = checkudata_raw (luaState, index, "luaNet_class") as byte[];
				if (udata != null)
					return fourBytesToInt (udata);
				
				udata = checkudata_raw (luaState, index, "luaNet_searchbase") as byte[];
				if (udata != null)
					return fourBytesToInt (udata);
				
				udata = checkudata_raw (luaState, index, "luaNet_function") as byte[];
				if (udata != null)
					return fourBytesToInt (udata);
			}
			
			return -1;
		}

		public static void luanet_newudata (lua_State luaState, int val)
		{
			var userdata = lua_newuserdata (luaState, sizeof(int)) as byte[];
			intToFourBytes (val, userdata);
		}

		public static int luanet_rawnetobj (lua_State luaState, int obj)
		{
			byte[] bytes = lua_touserdata (luaState, obj) as byte[];
			return fourBytesToInt (bytes);
		}

		private static int fourBytesToInt (byte [] bytes)
		{
			return bytes [0] + (bytes [1] << 8) + (bytes [2] << 16) + (bytes [3] << 24);
		}

		private static void intToFourBytes (int val, byte [] bytes)
		{
			// gfoot: is this really a good idea?
			bytes [0] = (byte)val;
			bytes [1] = (byte)(val >> 8);
			bytes [2] = (byte)(val >> 16);
			bytes [3] = (byte)(val >> 24);
		}

		/* Compatibility functions to allow NLua work with Lua 5.1.5 and Lua 5.2.2 with the same dll interface.
		 * KopiLua methods to match KeraLua API */ 

		public static int luanet_registryindex ()
		{
			return LUA_REGISTRYINDEX;
		}

		public static void luanet_pushglobaltable (lua_State L) 
		{
			lua_pushvalue (L, LUA_GLOBALSINDEX);
		}

		public static void luanet_popglobaltable (lua_State L)
		{
			lua_replace (L, LUA_GLOBALSINDEX);
		}

		public static void luanet_setglobal (lua_State L, string name)
		{
			lua_setglobal (L, name);
		}

		public static void luanet_getglobal (lua_State L, string name)
		{
			lua_getglobal (L, name);
		}
		
		public static int luanet_pcall (lua_State L, int nargs, int nresults, int errfunc)
		{
			return lua_pcall (L, nargs, nresults, errfunc);
		}

		[CLSCompliantAttribute (false)]
		public static int luanet_loadbuffer (lua_State L, string buff, uint sz, string name)
		{
			return luaL_loadbuffer (L, buff, sz, name);
		}

		[CLSCompliantAttribute (false)]
		public static int luanet_loadbuffer (lua_State L, byte [] buff, uint sz, string name)
		{
			return luaL_loadbuffer (L, buff, sz, name);
		}

		public static int luanet_loadfile (lua_State L, string file)
		{
			return luaL_loadfile (L, file);
		}

		public static double luanet_tonumber (lua_State L, int idx)
		{
			return lua_tonumber (L, idx);
		}

		public static int luanet_equal (lua_State L, int idx1, int idx2)
		{
			return lua_equal (L, idx1, idx2);
		}

		[CLSCompliantAttribute (false)]
		public static void luanet_pushlstring (lua_State L, string s, uint len)
		{
			lua_pushlstring (L, s, len);
		}
	}
}

