export type SocialAuthProvider = "google" | "apple";

export type GoogleAuthRequest = {
  provider: SocialAuthProvider;
  credential: string;
};

type SocialSignInResponse = {
  id: string;
  email: string;
  displayName: string;
  provider: string;
};

export function getGoogleClientId(): string {
  return import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim() ?? "";
}

export async function signInWithGoogle(credential: string): Promise<SocialSignInResponse> {
  return submitGoogleAuth("/api/auth/social", credential);
}

export async function createAccountWithGoogle(credential: string): Promise<SocialSignInResponse> {
  return submitGoogleAuth("/api/auth/social/register", credential);
}

async function submitGoogleAuth(path: string, credential: string): Promise<SocialSignInResponse> {
  const response = await fetch(path, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      provider: "google",
      credential
    } satisfies GoogleAuthRequest)
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Unable to sign in with Google.");
  }

  return response.json() as Promise<SocialSignInResponse>;
}
