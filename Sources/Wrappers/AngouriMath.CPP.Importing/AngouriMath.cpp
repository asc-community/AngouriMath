/*
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT.
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

#include "AngouriMath.h"
#include "Imports.h"
#include <vector>

namespace AngouriMath
{
    Entity CreateByHandle(Internal::EntityRef handle)
    {
        return Entity(handle);
    }

    namespace Internal
    {
        std::function<std::vector<AngouriMath::Entity>(AngouriMath::Internal::EntityRef)> GetLambdaByArrayFactory(std::function<NativeErrorCode(EntityRef, NativeArray*)> factory)
        {
            auto res = [factory = std::move(factory)](AngouriMath::Internal::EntityRef _this)->std::vector<AngouriMath::Entity> {
                NativeArray nRes;
                HandleErrorCode(factory(_this, &nRes));
                std::vector<Entity> res(nRes.length);
                for (size_t i = 0; i < nRes.length; i++)
                    res[i] = CreateByHandle(nRes.refs[i]);
                free_native_array(nRes);
                return res;
            };
            auto fun = std::function<std::vector<AngouriMath::Entity>(AngouriMath::Internal::EntityRef)>{ res };
            return res;
        }
    }

    struct HandleDeleter
    {
        void operator()(const Internal::EntityInstance* inner)
        {
            if (inner != nullptr)
            {
                (void)free_entity(inner->reference);
                delete inner;
            }
        }
    };

    Entity::Entity(Internal::EntityRef handle)
        : innerEntityInstance(new Internal::EntityInstance(handle), HandleDeleter())
    {
        
    }

    Internal::EntityRef ParseString(const char* expr)
    {
        Internal::EntityRef result;
        HandleErrorCode(maths_from_string(expr, &result));
        return result;
    }

    Entity::Entity()
        : innerEntityInstance(nullptr, HandleDeleter())
    {
    }

    Entity::Entity(const std::string& expr)
        : Entity(expr.c_str())
    {
    }

    Entity::Entity(const char* expr)
        : Entity(ParseString(expr))
    {
    }

    std::string Entity::ToString() const
    {
        char* buff = nullptr;
        HandleErrorCode(entity_to_string(innerEntityInstance.get()->reference, &buff));
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }

    std::string Entity::Latexise() const
    {
        char* buff = nullptr;
        HandleErrorCode(entity_latexise(innerEntityInstance.get()->reference, &buff));
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }


    Entity Entity::Differentiate(const Entity& var) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_differentiate(innerEntityInstance.get()->reference, var.innerEntityInstance.get()->reference, &result));
        return Entity(result);
    }

    Entity Entity::Integrate(const Entity& var) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_integrate(innerEntityInstance.get()->reference, var.innerEntityInstance.get()->reference, &result));
        return Entity(result);
    }


    Entity Entity::Limit(const Entity& var, const Entity& dest, ApproachFrom from) const
    {
        Internal::EntityRef result;
        HandleErrorCode(
            entity_limit(
                innerEntityInstance.get()->reference,
                var.innerEntityInstance.get()->reference,
                dest.innerEntityInstance.get()->reference,
                (Internal::ApproachFrom)from,
                &result
            )
        );
        return Entity(result);
    }

    Entity Entity::Limit(const Entity& var, const Entity& dest) const
    {
        return Limit(var, dest, ApproachFrom::BothSides);
    }

    Entity Entity::Simplify() const
    {
        Internal::EntityRef res;
        HandleErrorCode(entity_simplify(innerEntityInstance.get()->reference, &res));
        return Entity(res);
    }

    std::vector<Entity> Entity::Alternate() const
    {
        auto lambda = Internal::GetLambdaByArrayFactory(entity_alternate);
        return lambda(innerEntityInstance.get()->reference);
    }

    long Entity::AsInteger() const
    {
        long res;
        HandleErrorCode(entity_to_long(innerEntityInstance.get()->reference, &res));
        return res;
    }

    std::pair<long, long> Entity::AsRational() const
    {
        Internal::LongTuple res;
        HandleErrorCode(entity_to_rational(innerEntityInstance.get()->reference, &res));
        return std::make_pair(res.first, res.second);
    }

    double Entity::AsReal() const
    {
        double res;
        HandleErrorCode(entity_to_double(innerEntityInstance.get()->reference, &res));
        return res;
    }

    std::pair<double, double> Entity::AsComplex() const
    {
        Internal::DoubleTuple res;
        HandleErrorCode(entity_to_complex(innerEntityInstance.get()->reference, &res));
        return std::make_pair(res.first, res.second);
    }

    Internal::EntityRef GetHandle(const Entity& e)
    {
        return e.innerEntityInstance.get()->reference;
    }

    namespace Internal
    {
        const std::vector<Entity>& EntityInstance::CachedNodes()
        {
            const auto& res = nodes.GetValue(GetLambdaByArrayFactory(entity_nodes), reference);
            return res;
        }

        const std::vector<Entity>& EntityInstance::CachedVars()
        {
            const auto& res = vars.GetValue(GetLambdaByArrayFactory(entity_vars), reference);
            return res;
        }

        const std::vector<Entity>& EntityInstance::CachedVarsAndConstants()
        {
            const auto& res = varsAndConstants.GetValue(GetLambdaByArrayFactory(entity_vars_and_constants), reference);
            return res;
        }

        const Entity& EntityInstance::CachedEvaled()
        {
            std::function<std::shared_ptr<Entity>(EntityRef)> fact = [](EntityRef ref) -> std::shared_ptr<Entity> {
                EntityRef res;
                HandleErrorCode(entity_evaled(ref, &res));
                return std::make_shared<Entity>(CreateByHandle(res));
            };
            const auto& res = innerEvaled.GetValue(fact, reference);
            return (const Entity&)res;
        }

        const Entity& EntityInstance::CachedInnerSimplified()
        {
            std::function<std::shared_ptr<Entity>(EntityRef)> fact = [](EntityRef ref) -> std::shared_ptr<Entity> {
                EntityRef res;
                HandleErrorCode(entity_evaled(ref, &res));
                return std::make_shared<Entity>(CreateByHandle(res));
            };
            const auto& res = innerSimplified.GetValue(fact, reference);
            return (const Entity&)res;
        }
    }
}