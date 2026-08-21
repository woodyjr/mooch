export type AppleAuthRequest = {
  provider: "apple";
  credential: string;
};

type SocialSignInResponse = {
  id: string;
  email: string;
  displayName: string;
  provider: string;
};

export async function signInWithApple(credential: string): Promise<SocialSignInResponse> {
  void credential;

  throw new Error("Apple sign-in is not wired to verified OAuth tokens yet.");
}
