import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode
} from "react";

import { createAccountWithGoogle, signInWithApple, signInWithGoogle } from "./providers";

export type AuthProviderName = "email" | "google" | "apple";

export type AuthUser = {
  id: string;
  email: string;
  displayName: string;
  provider: AuthProviderName;
};

type AuthApiResponse = {
  id: string;
  email: string;
  displayName: string;
  provider: string;
};

type AuthContextValue = {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<AuthUser>;
  register: (email: string, displayName: string, password: string) => Promise<AuthUser>;
  socialLogin: (provider: Extract<AuthProviderName, "google" | "apple">, credential: string) => Promise<AuthUser>;
  socialRegister: (provider: Extract<AuthProviderName, "google" | "apple">, credential: string) => Promise<AuthUser>;
  logout: () => Promise<void>;
};

const AUTH_STORAGE_KEY = "mooch-auth-user";
const AuthContext = createContext<AuthContextValue | null>(null);

function readStoredUser(): AuthUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const storedValue = window.localStorage.getItem(AUTH_STORAGE_KEY);

  if (!storedValue) {
    return null;
  }

  try {
    return JSON.parse(storedValue) as AuthUser;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

function toAuthUser(payload: AuthApiResponse): AuthUser {
  return {
    id: payload.id,
    email: payload.email,
    displayName: payload.displayName,
    provider: payload.provider === "google" ? "google" : payload.provider === "apple" ? "apple" : "email"
  };
}

async function fetchCurrentUser(): Promise<AuthUser | null> {
  const response = await fetch("/api/auth/me", {
    method: "GET",
    credentials: "include",
    headers: {
      Accept: "application/json"
    }
  });

  if (response.status === 401) {
    return null;
  }

  if (!response.ok) {
    throw new Error("Unable to load your current session.");
  }

  const payload = (await response.json()) as AuthApiResponse;
  return toAuthUser(payload);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(readStoredUser);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function hydrateSession() {
      try {
        const sessionUser = await fetchCurrentUser();
        setUser(sessionUser);
      } catch {
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    }

    void hydrateSession();
  }, []);

  useEffect(() => {
    if (user) {
      window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
      return;
    }

    window.localStorage.removeItem(AUTH_STORAGE_KEY);
  }, [user]);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isAuthenticated: user !== null,
    isLoading,
    login: async (email, password) => {
      const response = await fetch("/api/auth/login", {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
      });

      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Unable to sign in.");
      }

      const payload = (await response.json()) as AuthApiResponse;
      const nextUser = toAuthUser(payload);
      setUser(nextUser);
      return nextUser;
    },
    register: async (email, displayName, password) => {
      const response = await fetch("/api/auth/register", {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, displayName, password })
      });

      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Unable to create an account.");
      }

      const payload = (await response.json()) as AuthApiResponse;
      const nextUser = toAuthUser(payload);
      setUser(nextUser);
      return nextUser;
    },
    socialLogin: async (provider, credential) => {
      const payload = provider === "google"
        ? await signInWithGoogle(credential)
        : await signInWithApple(credential);

      const nextUser = toAuthUser(payload);
      setUser(nextUser);
      return nextUser;
    },
    socialRegister: async (provider, credential) => {
      const payload = provider === "google"
        ? await createAccountWithGoogle(credential)
        : await signInWithApple(credential);

      const nextUser = toAuthUser(payload);
      setUser(nextUser);
      return nextUser;
    },
    logout: async () => {
      try {
        await fetch("/api/auth/logout", {
          method: "POST",
          credentials: "include"
        });
      } finally {
        setUser(null);
      }
    }
  }), [isLoading, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}
