import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { NavLink, Outlet, useNavigate } from "react-router-dom";

import { useAuth } from "../../features/auth/auth-context";
import { acceptDogOwnerInvite, declineDogOwnerInvite, fetchPendingDogOwnerInvites } from "../../features/dogs/api";
import { Button, buttonClassName } from "../ui/button";
import { MoochIcon } from "../ui/mooch-icon";

const navigation = [
  { to: "/app", label: "Home", icon: "home" as const },
  { to: "/app/pack", label: "Pack", icon: "pack" as const },
  { to: "/app/walk", label: "Walk", icon: "walk" as const },
  { to: "/app/ranks", label: "Ranks", icon: "ranks" as const },
  { to: "/app/dogs", label: "My Dogs", icon: "dog" as const }
];

export function AppShell() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { logout } = useAuth();
  const { data: pendingInvites = [] } = useQuery({
    queryKey: ["pending-dog-owner-invites"],
    queryFn: fetchPendingDogOwnerInvites
  });

  const acceptInviteMutation = useMutation({
    mutationFn: acceptDogOwnerInvite,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["pending-dog-owner-invites"] });
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      await queryClient.invalidateQueries({ queryKey: ["dog-dashboard"] });
    }
  });

  const declineInviteMutation = useMutation({
    mutationFn: declineDogOwnerInvite,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["pending-dog-owner-invites"] });
    }
  });

  async function handleLogout() {
    await logout();
    navigate("/");
  }

  return (
    <div className="app-shell">
      <header className="app-shell__header">
        <div className="app-shell__brand">
          <div className="app-shell__brand-mark">
            <MoochIcon name="paw" />
          </div>
          <div>
            <p className="app-shell__brand-name">Mooch</p>
            <p className="app-shell__brand-tagline">Social fitness for dogs</p>
          </div>
        </div>

        <div className="app-shell__actions">
          <Button aria-label="Notifications" variant="icon">
            <MoochIcon name="bell" />
          </Button>
          <NavLink className={buttonClassName({ variant: "secondary" })} to="/app/dogs">
            My Dogs
          </NavLink>
          <NavLink className={buttonClassName()} to="/app/walk">
            + Start walk
          </NavLink>
          <Button onClick={() => void handleLogout()} type="button" variant="secondary">
            Log out
          </Button>
        </div>
      </header>

      <main className="app-shell__content">
        {pendingInvites.length > 0 && (
          <section className="invite-banner">
            <div className="invite-banner__header">
              <div>
                <p className="eyebrow">Dog invite</p>
                <h2>You have {pendingInvites.length} dog invite{pendingInvites.length === 1 ? "" : "s"} waiting</h2>
              </div>
              <NavLink className="section-link" to="/app/dogs">
                View dogs
              </NavLink>
            </div>

            <div className="invite-banner__list">
              {pendingInvites.map((invite) => (
                <article className="invite-banner__item" key={invite.id}>
                  <div>
                    <h3>Join {invite.dogName}</h3>
                    <p>{invite.invitedByDisplayName} invited you to the shared dog profile on {new Date(invite.createdDateUtc).toLocaleDateString("en-US", { month: "long", day: "numeric" })}.</p>
                  </div>
                  <div className="invite-banner__actions">
                    <Button
                      disabled={declineInviteMutation.isPending}
                      onClick={() => void declineInviteMutation.mutateAsync(invite.id)}
                      type="button"
                      variant="secondary"
                    >
                      Not now
                    </Button>
                    <Button
                      disabled={acceptInviteMutation.isPending}
                      onClick={() => void acceptInviteMutation.mutateAsync(invite.id)}
                      type="button"
                    >
                      Join dog
                    </Button>
                  </div>
                </article>
              ))}
            </div>
          </section>
        )}

        <Outlet />
      </main>

      <nav className="bottom-nav">
        {navigation.map((item) => (
          <NavLink
            className={({ isActive }) => {
              const classes = ["bottom-nav__link"];
              if (item.label === "Walk") {
                classes.push("bottom-nav__link--walk");
              }

              if (isActive) {
                classes.push("bottom-nav__link--active");
              }

              return classes.join(" ");
            }}
            end={item.to === "/app"}
            key={item.to}
            to={item.to}
          >
            <span className="bottom-nav__icon">
              <MoochIcon name={item.icon} />
            </span>
            <span className="bottom-nav__label">{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
