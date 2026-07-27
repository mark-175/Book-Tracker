import { useCallback, useEffect, useState } from "react";
import * as authApi from "@/api/auth";
import type { UserDTO } from "@/types/api";

type AuthStatus = "loading" | "authenticated" | "unauthenticated";

export function useAuth() {
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [user, setUser] = useState<UserDTO | null>(null);

  const checkSession = useCallback(async (): Promise<boolean> => {
    try {
      const me = await authApi.getMe();
      setUser(me);
      setStatus(me ? "authenticated" : "unauthenticated");
      return me !== null;
    } catch (err) {
      console.error(err);
      setUser(null);
      setStatus("unauthenticated");
      return false;
    }
  }, []);

  useEffect(() => {
    checkSession();
  }, [checkSession]);

  useEffect(() => {
    const handleUnauthorized = () => {
      setUser(null);
      setStatus("unauthenticated");
    };
    window.addEventListener("auth:unauthorized", handleUnauthorized);
    return () =>
      window.removeEventListener("auth:unauthorized", handleUnauthorized);
  }, []);

  const login = useCallback(
    async (username: string, password: string) => {
      await authApi.login(username, password);
      const authenticated = await checkSession();
      if (!authenticated) {
        throw new Error(
          "Signed in, but couldn't establish a session. Please try again.",
        );
      }
    },
    [checkSession],
  );

  const register = useCallback(async (username: string, password: string) => {
    await authApi.register(username, password);
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout();
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  return { status, user, login, register, logout };
}
